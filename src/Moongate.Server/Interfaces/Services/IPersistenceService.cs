using Moongate.Abstractions.Interfaces.Services.Base;
using Moongate.Persistence.Interfaces.Persistence;

namespace Moongate.Server.Interfaces.Services;

/// <summary>
/// Exposes persistence lifecycle operations for the game server host.
/// </summary>
public interface IPersistenceService : IMoongateService, IDisposable
{
    /// <summary>
    /// Gets the underlying unit of work used by runtime services.
    /// </summary>
    IPersistenceUnitOfWork UnitOfWork { get; }

    /// <summary>
    /// Saves the current in-memory state to snapshot storage.
    /// </summary>
    Task SaveAsync(CancellationToken cancellationToken = default);
}
