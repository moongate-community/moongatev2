using Moongate.UO.Data.Ids;

namespace Moongate.Persistence.Interfaces.Persistence;

/// <summary>
/// Coordinates repositories and persistence lifecycle for world state.
/// </summary>
public interface IPersistenceUnitOfWork
{
    /// <summary>
    /// Gets the account repository.
    /// </summary>
    IAccountRepository Accounts { get; }

    /// <summary>
    /// Gets the mobile repository.
    /// </summary>
    IMobileRepository Mobiles { get; }

    /// <summary>
    /// Gets the item repository.
    /// </summary>
    IItemRepository Items { get; }

    /// <summary>
    /// Allocates the next progressive account serial identifier.
    /// </summary>
    Serial AllocateNextAccountId();

    /// <summary>
    /// Allocates the next progressive item serial identifier.
    /// </summary>
    Serial AllocateNextItemId();

    /// <summary>
    /// Allocates the next progressive mobile serial identifier.
    /// </summary>
    Serial AllocateNextMobileId();

    /// <summary>
    /// Loads state from snapshot and replays journal entries.
    /// </summary>
    ValueTask InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a new snapshot and resets the journal.
    /// </summary>
    ValueTask SaveSnapshotAsync(CancellationToken cancellationToken = default);
}
