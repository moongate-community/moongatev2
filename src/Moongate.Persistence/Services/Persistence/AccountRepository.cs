using Moongate.Persistence.Data.Internal;
using Moongate.Persistence.Data.Persistence;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Persistence.Types;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;
using Serilog;
using ZLinq;

namespace Moongate.Persistence.Services.Persistence;

/// <summary>
/// Thread-safe account repository backed by the shared persistence state store.
/// </summary>
public sealed class AccountRepository : IAccountRepository
{
    private readonly IJournalService _journalService;
    private readonly PersistenceStateStore _stateStore;
    private readonly ILogger _logger = Log.ForContext<AccountRepository>();

    internal AccountRepository(PersistenceStateStore stateStore, IJournalService journalService)
    {
        _stateStore = stateStore;
        _journalService = journalService;
    }

    public async ValueTask<bool> AddAsync(UOAccountEntity account, CancellationToken cancellationToken = default)
    {
        _logger.Verbose("Account add requested for Id={AccountId} Username={Username}", account.Id, account.Username);
        var normalizedUsername = account.Username.Trim();

        bool inserted;
        JournalEntry? entry = null;

        lock (_stateStore.SyncRoot)
        {
            if (_stateStore.AccountsById.ContainsKey(account.Id) ||
                _stateStore.AccountNameIndex.ContainsKey(normalizedUsername))
            {
                inserted = false;
            }
            else
            {
                var clone = Clone(account);
                clone.Username = normalizedUsername;
                _stateStore.AccountsById[clone.Id] = clone;
                _stateStore.AccountNameIndex[clone.Username] = clone.Id;
                _stateStore.LastAccountId = Math.Max(_stateStore.LastAccountId, (uint)clone.Id);

                inserted = true;
                entry = CreateEntry(PersistenceOperationType.UpsertAccount, JournalPayloadCodec.EncodeAccount(clone));
            }
        }

        if (inserted && entry is not null)
        {
            await _journalService.AppendAsync(entry, cancellationToken);
        }

        _logger.Verbose(
            "Account add completed for Id={AccountId} Username={Username} Inserted={Inserted}",
            account.Id,
            normalizedUsername,
            inserted
        );

        return inserted;
    }

    public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
    {
        _logger.Verbose("Account count requested");
        cancellationToken.ThrowIfCancellationRequested();

        lock (_stateStore.SyncRoot)
        {
            var count = _stateStore.AccountsById.Count;
            _logger.Verbose("Account count completed Count={Count}", count);

            return ValueTask.FromResult(count);
        }
    }

    public ValueTask<bool> ExistsAsync(
        Func<UOAccountEntity, bool> predicate,
        CancellationToken cancellationToken = default
    )
    {
        _logger.Verbose("Account exists query requested");
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(predicate);

        lock (_stateStore.SyncRoot)
        {
            var exists = _stateStore.AccountsById.Values.AsValueEnumerable().Any(predicate);
            _logger.Verbose("Account exists query completed Exists={Exists}", exists);

            return ValueTask.FromResult(exists);
        }
    }

    public ValueTask<IReadOnlyCollection<UOAccountEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.Verbose("Account get-all requested");
        _ = cancellationToken;

        lock (_stateStore.SyncRoot)
        {
            return ValueTask.FromResult<IReadOnlyCollection<UOAccountEntity>>(
                [
                    .. _stateStore.AccountsById.Values.Select(Clone)
                ]
            );
        }
    }

    public ValueTask<UOAccountEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
    {
        _logger.Verbose("Account get-by-id requested for Id={AccountId}", id);
        _ = cancellationToken;

        lock (_stateStore.SyncRoot)
        {
            return ValueTask.FromResult(_stateStore.AccountsById.TryGetValue(id, out var account) ? Clone(account) : null);
        }
    }

    public ValueTask<UOAccountEntity?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        _logger.Verbose("Account get-by-username requested for Username={Username}", username);
        _ = cancellationToken;

        lock (_stateStore.SyncRoot)
        {
            if (!_stateStore.AccountNameIndex.TryGetValue(username.Trim(), out var serial))
            {
                return ValueTask.FromResult<UOAccountEntity?>(null);
            }

            return ValueTask.FromResult(
                _stateStore.AccountsById.TryGetValue(serial, out var account) ? Clone(account) : null
            );
        }
    }

    public ValueTask<IReadOnlyList<TResult>> QueryAsync<TResult>(
        Func<UOAccountEntity, bool> predicate,
        Func<UOAccountEntity, TResult> selector,
        CancellationToken cancellationToken = default
    )
    {
        _logger.Verbose("Account query requested");
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(selector);

        UOAccountEntity[] snapshot;

        lock (_stateStore.SyncRoot)
        {
            snapshot = [.. _stateStore.AccountsById.Values.Select(Clone)];
        }

        var results = snapshot.AsValueEnumerable().Where(predicate).Select(selector).ToArray();
        _logger.Verbose("Account query completed with Count={Count}", results.Length);

        return ValueTask.FromResult<IReadOnlyList<TResult>>(results);
    }

    public async ValueTask<bool> RemoveAsync(Serial id, CancellationToken cancellationToken = default)
    {
        _logger.Verbose("Account remove requested for Id={AccountId}", id);
        var removed = false;
        JournalEntry? entry = null;

        lock (_stateStore.SyncRoot)
        {
            if (_stateStore.AccountsById.Remove(id, out var existing))
            {
                _stateStore.AccountNameIndex.Remove(existing.Username);
                removed = true;
                entry = CreateEntry(PersistenceOperationType.RemoveAccount, JournalPayloadCodec.EncodeSerial(id));
            }
        }

        if (removed && entry is not null)
        {
            await _journalService.AppendAsync(entry, cancellationToken);
        }

        _logger.Verbose("Account remove completed for Id={AccountId} Removed={Removed}", id, removed);

        return removed;
    }

    public async ValueTask UpsertAsync(UOAccountEntity account, CancellationToken cancellationToken = default)
    {
        _logger.Verbose("Account upsert requested for Id={AccountId} Username={Username}", account.Id, account.Username);
        var normalizedUsername = account.Username.Trim();
        JournalEntry entry;

        lock (_stateStore.SyncRoot)
        {
            var clone = Clone(account);
            clone.Username = normalizedUsername;

            if (_stateStore.AccountsById.TryGetValue(clone.Id, out var existing) &&
                !existing.Username.Equals(clone.Username, StringComparison.OrdinalIgnoreCase))
            {
                _stateStore.AccountNameIndex.Remove(existing.Username);
            }

            _stateStore.AccountsById[clone.Id] = clone;
            _stateStore.AccountNameIndex[clone.Username] = clone.Id;
            _stateStore.LastAccountId = Math.Max(_stateStore.LastAccountId, (uint)clone.Id);

            entry = CreateEntry(PersistenceOperationType.UpsertAccount, JournalPayloadCodec.EncodeAccount(clone));
        }

        await _journalService.AppendAsync(entry, cancellationToken);
        _logger.Verbose("Account upsert completed for Id={AccountId} Username={Username}", account.Id, normalizedUsername);
    }

    private static UOAccountEntity Clone(UOAccountEntity account)
        => SnapshotMapper.ToAccountEntity(SnapshotMapper.ToAccountSnapshot(account));

    private JournalEntry CreateEntry(PersistenceOperationType operationType, byte[] payload)
        => new()
        {
            SequenceId = ++_stateStore.LastSequenceId,
            TimestampUnixMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            OperationType = operationType,
            Payload = payload
        };
}
