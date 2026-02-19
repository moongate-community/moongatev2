using Moongate.Persistence.Data.Internal;
using Moongate.Persistence.Data.Persistence;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Persistence.Types;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.Persistence.Services.Persistence;

/// <summary>
/// Thread-safe account repository backed by the shared persistence state store.
/// </summary>
public sealed class AccountRepository : IAccountRepository
{
    private readonly IJournalService _journalService;
    private readonly PersistenceStateStore _stateStore;

    internal AccountRepository(PersistenceStateStore stateStore, IJournalService journalService)
    {
        _stateStore = stateStore;
        _journalService = journalService;
    }

    public async ValueTask<bool> AddAsync(UOAccountEntity account, CancellationToken cancellationToken = default)
    {
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

                inserted = true;
                entry = CreateEntry(PersistenceOperationType.UpsertAccount, JournalPayloadCodec.EncodeAccount(clone));
            }
        }

        if (inserted && entry is not null)
        {
            await _journalService.AppendAsync(entry, cancellationToken);
        }

        return inserted;
    }

    public ValueTask<IReadOnlyCollection<UOAccountEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
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
        _ = cancellationToken;

        lock (_stateStore.SyncRoot)
        {
            return ValueTask.FromResult(_stateStore.AccountsById.TryGetValue(id, out var account) ? Clone(account) : null);
        }
    }

    public ValueTask<UOAccountEntity?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
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

    public async ValueTask<bool> RemoveAsync(Serial id, CancellationToken cancellationToken = default)
    {
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

        return removed;
    }

    public async ValueTask UpsertAsync(UOAccountEntity account, CancellationToken cancellationToken = default)
    {
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

            entry = CreateEntry(PersistenceOperationType.UpsertAccount, JournalPayloadCodec.EncodeAccount(clone));
        }

        await _journalService.AppendAsync(entry, cancellationToken);
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
