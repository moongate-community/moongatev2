namespace Moongate.Server.Interfaces.Services.Lifecycle;

/// <summary>
/// Exposes server stop signaling shared across services.
/// </summary>
public interface IServerLifetimeService
{
    /// <summary>
    /// Gets a token cancelled when shutdown is requested.
    /// </summary>
    CancellationToken ShutdownToken { get; }

    /// <summary>
    /// Requests the server shutdown sequence.
    /// </summary>
    void RequestShutdown();
}
