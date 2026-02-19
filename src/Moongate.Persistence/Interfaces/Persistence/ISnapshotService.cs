using Moongate.Persistence.Data.Persistence;

namespace Moongate.Persistence.Interfaces.Persistence;

/// <summary>
/// Reads and writes complete world snapshots.
/// </summary>
public interface ISnapshotService
{
    /// <summary>
    /// Loads the latest persisted world snapshot.
    /// </summary>
    ValueTask<WorldSnapshot?> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a full world snapshot.
    /// </summary>
    ValueTask SaveAsync(WorldSnapshot snapshot, CancellationToken cancellationToken = default);
}
