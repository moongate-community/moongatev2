using Moongate.Persistence.Data.Internal;
using Moongate.Persistence.Data.Persistence;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Persistence.Types;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;
using ZLinq;

namespace Moongate.Persistence.Services.Persistence;

/// <summary>
/// Thread-safe item repository backed by the shared persistence state store.
/// </summary>
public sealed class ItemRepository : IItemRepository
{
    private readonly IJournalService _journalService;
    private readonly PersistenceStateStore _stateStore;

    internal ItemRepository(PersistenceStateStore stateStore, IJournalService journalService)
    {
        _stateStore = stateStore;
        _journalService = journalService;
    }

    public ValueTask<IReadOnlyCollection<UOItemEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
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
        _ = cancellationToken;

        lock (_stateStore.SyncRoot)
        {
            return ValueTask.FromResult(_stateStore.ItemsById.TryGetValue(id, out var item) ? Clone(item) : null);
        }
    }

    public async ValueTask<bool> RemoveAsync(Serial id, CancellationToken cancellationToken = default)
    {
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

        return removed;
    }

    public async ValueTask UpsertAsync(UOItemEntity item, CancellationToken cancellationToken = default)
    {
        JournalEntry entry;

        lock (_stateStore.SyncRoot)
        {
            var clone = Clone(item);
            _stateStore.ItemsById[clone.Id] = clone;
            entry = CreateEntry(PersistenceOperationType.UpsertItem, JournalPayloadCodec.EncodeItem(clone));
        }

        await _journalService.AppendAsync(entry, cancellationToken);
    }

    public ValueTask<IReadOnlyList<TResult>> QueryAsync<TResult>(
        Func<UOItemEntity, bool> predicate,
        Func<UOItemEntity, TResult> selector,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(selector);

        UOItemEntity[] snapshot;

        lock (_stateStore.SyncRoot)
        {
            snapshot = [.. _stateStore.ItemsById.Values.Select(Clone)];
        }

        var results = snapshot.AsValueEnumerable().Where(predicate).Select(selector).ToArray();
        return ValueTask.FromResult<IReadOnlyList<TResult>>(results);
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
