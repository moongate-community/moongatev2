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
/// Thread-safe mobile repository backed by the shared persistence state store.
/// </summary>
public sealed class MobileRepository : IMobileRepository
{
    private readonly IJournalService _journalService;
    private readonly PersistenceStateStore _stateStore;
    private readonly ILogger _logger = Log.ForContext<MobileRepository>();

    internal MobileRepository(PersistenceStateStore stateStore, IJournalService journalService)
    {
        _stateStore = stateStore;
        _journalService = journalService;
    }

    public ValueTask<IReadOnlyCollection<UOMobileEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.Verbose("Mobile get-all requested");
        _ = cancellationToken;

        lock (_stateStore.SyncRoot)
        {
            return ValueTask.FromResult<IReadOnlyCollection<UOMobileEntity>>(
                [
                    .. _stateStore.MobilesById.Values.Select(Clone)
                ]
            );
        }
    }

    public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
    {
        _logger.Verbose("Mobile count requested");
        cancellationToken.ThrowIfCancellationRequested();

        lock (_stateStore.SyncRoot)
        {
            var count = _stateStore.MobilesById.Count;
            _logger.Verbose("Mobile count completed Count={Count}", count);
            return ValueTask.FromResult(count);
        }
    }

    public ValueTask<UOMobileEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
    {
        _logger.Verbose("Mobile get-by-id requested for Id={MobileId}", id);
        _ = cancellationToken;

        lock (_stateStore.SyncRoot)
        {
            return ValueTask.FromResult(_stateStore.MobilesById.TryGetValue(id, out var mobile) ? Clone(mobile) : null);
        }
    }

    public async ValueTask<bool> RemoveAsync(Serial id, CancellationToken cancellationToken = default)
    {
        _logger.Verbose("Mobile remove requested for Id={MobileId}", id);
        var removed = false;
        JournalEntry? entry = null;

        lock (_stateStore.SyncRoot)
        {
            if (_stateStore.MobilesById.Remove(id))
            {
                removed = true;
                entry = CreateEntry(PersistenceOperationType.RemoveMobile, JournalPayloadCodec.EncodeSerial(id));
            }
        }

        if (removed && entry is not null)
        {
            await _journalService.AppendAsync(entry, cancellationToken);
        }

        _logger.Verbose("Mobile remove completed for Id={MobileId} Removed={Removed}", id, removed);

        return removed;
    }

    public async ValueTask UpsertAsync(UOMobileEntity mobile, CancellationToken cancellationToken = default)
    {
        _logger.Verbose("Mobile upsert requested for Id={MobileId}", mobile.Id);
        JournalEntry entry;

        lock (_stateStore.SyncRoot)
        {
            var clone = Clone(mobile);
            _stateStore.MobilesById[clone.Id] = clone;
            _stateStore.LastMobileId = Math.Max(_stateStore.LastMobileId, (uint)clone.Id);
            entry = CreateEntry(PersistenceOperationType.UpsertMobile, JournalPayloadCodec.EncodeMobile(clone));
        }

        await _journalService.AppendAsync(entry, cancellationToken);
        _logger.Verbose("Mobile upsert completed for Id={MobileId}", mobile.Id);
    }

    public ValueTask<IReadOnlyList<TResult>> QueryAsync<TResult>(
        Func<UOMobileEntity, bool> predicate,
        Func<UOMobileEntity, TResult> selector,
        CancellationToken cancellationToken = default
    )
    {
        _logger.Verbose("Mobile query requested");
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(selector);

        UOMobileEntity[] snapshot;

        lock (_stateStore.SyncRoot)
        {
            snapshot = [.. _stateStore.MobilesById.Values.Select(Clone)];
        }

        var results = snapshot.AsValueEnumerable().Where(predicate).Select(selector).ToArray();
        _logger.Verbose("Mobile query completed with Count={Count}", results.Length);
        return ValueTask.FromResult<IReadOnlyList<TResult>>(results);
    }

    private static UOMobileEntity Clone(UOMobileEntity mobile)
        => SnapshotMapper.ToMobileEntity(SnapshotMapper.ToMobileSnapshot(mobile));

    private JournalEntry CreateEntry(PersistenceOperationType operationType, byte[] payload)
        => new()
        {
            SequenceId = ++_stateStore.LastSequenceId,
            TimestampUnixMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            OperationType = operationType,
            Payload = payload
        };
}
