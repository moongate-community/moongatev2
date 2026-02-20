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
/// Thread-safe item repository backed by the shared persistence state store.
/// </summary>
public sealed class ItemRepository : IItemRepository
{
    private readonly IJournalService _journalService;
    private readonly PersistenceStateStore _stateStore;
    private readonly ILogger _logger = Log.ForContext<ItemRepository>();

    internal ItemRepository(PersistenceStateStore stateStore, IJournalService journalService)
    {
        _stateStore = stateStore;
        _journalService = journalService;
    }

    public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
    {
        _logger.Verbose("Item count requested");
        cancellationToken.ThrowIfCancellationRequested();

        lock (_stateStore.SyncRoot)
        {
            var count = _stateStore.ItemsById.Count;
            _logger.Verbose("Item count completed Count={Count}", count);

            return ValueTask.FromResult(count);
        }
    }

    public ValueTask<IReadOnlyCollection<UOItemEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.Verbose("Item get-all requested");
        _ = cancellationToken;

        lock (_stateStore.SyncRoot)
        {
            return ValueTask.FromResult<IReadOnlyCollection<UOItemEntity>>(
                [
                    .. _stateStore.ItemsById.Values.Select(Clone)
                ]
            );
        }
    }

    public ValueTask<UOItemEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
    {
        _logger.Verbose("Item get-by-id requested for Id={ItemId}", id);
        _ = cancellationToken;

        lock (_stateStore.SyncRoot)
        {
            return ValueTask.FromResult(_stateStore.ItemsById.TryGetValue(id, out var item) ? Clone(item) : null);
        }
    }

    public ValueTask<IReadOnlyList<TResult>> QueryAsync<TResult>(
        Func<UOItemEntity, bool> predicate,
        Func<UOItemEntity, TResult> selector,
        CancellationToken cancellationToken = default
    )
    {
        _logger.Verbose("Item query requested");
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(selector);

        UOItemEntity[] snapshot;

        lock (_stateStore.SyncRoot)
        {
            snapshot = [.. _stateStore.ItemsById.Values.Select(Clone)];
        }

        var results = snapshot.AsValueEnumerable().Where(predicate).Select(selector).ToArray();
        _logger.Verbose("Item query completed with Count={Count}", results.Length);

        return ValueTask.FromResult<IReadOnlyList<TResult>>(results);
    }

    public async ValueTask<bool> RemoveAsync(Serial id, CancellationToken cancellationToken = default)
    {
        _logger.Verbose("Item remove requested for Id={ItemId}", id);
        var removed = false;
        JournalEntry? entry = null;

        lock (_stateStore.SyncRoot)
        {
            if (_stateStore.ItemsById.Remove(id))
            {
                removed = true;
                entry = CreateEntry(PersistenceOperationType.RemoveItem, JournalPayloadCodec.EncodeSerial(id));
            }
        }

        if (removed && entry is not null)
        {
            await _journalService.AppendAsync(entry, cancellationToken);
        }

        _logger.Verbose("Item remove completed for Id={ItemId} Removed={Removed}", id, removed);

        return removed;
    }

    public async ValueTask UpsertAsync(UOItemEntity item, CancellationToken cancellationToken = default)
    {
        _logger.Verbose("Item upsert requested for Id={ItemId}", item.Id);
        JournalEntry entry;

        lock (_stateStore.SyncRoot)
        {
            var clone = Clone(item);
            _stateStore.ItemsById[clone.Id] = clone;
            _stateStore.LastItemId = Math.Max(_stateStore.LastItemId, (uint)clone.Id);
            entry = CreateEntry(PersistenceOperationType.UpsertItem, JournalPayloadCodec.EncodeItem(clone));
        }

        await _journalService.AppendAsync(entry, cancellationToken);
        _logger.Verbose("Item upsert completed for Id={ItemId}", item.Id);
    }

    private static UOItemEntity Clone(UOItemEntity item)
        => SnapshotMapper.ToItemEntity(SnapshotMapper.ToItemSnapshot(item));

    private JournalEntry CreateEntry(PersistenceOperationType operationType, byte[] payload)
        => new()
        {
            SequenceId = ++_stateStore.LastSequenceId,
            TimestampUnixMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            OperationType = operationType,
            Payload = payload
        };
}
