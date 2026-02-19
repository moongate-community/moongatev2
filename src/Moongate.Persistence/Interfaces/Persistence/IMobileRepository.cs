using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.Persistence.Interfaces.Persistence;

/// <summary>
/// Provides persistence operations for mobile entities.
/// </summary>
public interface IMobileRepository
{
    /// <summary>
    /// Returns all persisted mobiles.
    /// </summary>
    ValueTask<IReadOnlyCollection<UOMobileEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the current number of persisted mobiles.
    /// </summary>
    ValueTask<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a mobile by its serial identifier.
    /// </summary>
    ValueTask<UOMobileEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a mobile by its serial identifier.
    /// </summary>
    ValueTask<bool> RemoveAsync(Serial id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts or updates a mobile.
    /// </summary>
    ValueTask UpsertAsync(UOMobileEntity mobile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a projection query over mobile entities.
    /// </summary>
    ValueTask<IReadOnlyList<TResult>> QueryAsync<TResult>(
        Func<UOMobileEntity, bool> predicate,
        Func<UOMobileEntity, TResult> selector,
        CancellationToken cancellationToken = default
    );
}
