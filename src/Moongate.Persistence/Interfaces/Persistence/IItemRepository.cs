using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.Persistence.Interfaces.Persistence;

/// <summary>
/// Provides persistence operations for item entities.
/// </summary>
public interface IItemRepository
{
    /// <summary>
    /// Returns the current number of persisted items.
    /// </summary>
    ValueTask<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all persisted items.
    /// </summary>
    ValueTask<IReadOnlyCollection<UOItemEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an item by its serial identifier.
    /// </summary>
    ValueTask<UOItemEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a projection query over item entities.
    /// </summary>
    ValueTask<IReadOnlyList<TResult>> QueryAsync<TResult>(
        Func<UOItemEntity, bool> predicate,
        Func<UOItemEntity, TResult> selector,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Removes an item by its serial identifier.
    /// </summary>
    ValueTask<bool> RemoveAsync(Serial id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts or updates an item.
    /// </summary>
    ValueTask UpsertAsync(UOItemEntity item, CancellationToken cancellationToken = default);
}
