namespace Moongate.Abstractions.Interfaces.Services;

/// <summary>
/// Defines the lifecycle contract for Moongate services managed by the host.
/// </summary>
public interface IMoongateService
{
    /// <summary>
    /// Starts the service lifecycle.
    /// </summary>
    /// <returns>A task that completes when startup logic has finished.</returns>
    Task StartAsync();

    /// <summary>
    /// Stops the service lifecycle.
    /// </summary>
    /// <returns>A task that completes when shutdown logic has finished.</returns>
    Task StopAsync();
}
