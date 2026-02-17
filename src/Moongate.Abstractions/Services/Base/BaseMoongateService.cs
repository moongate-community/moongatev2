using Moongate.Abstractions.Interfaces.Services.Base;

namespace Moongate.Abstractions.Services.Base;

/// <summary>
/// Provides a no-op base implementation for the Moongate service lifecycle.
/// </summary>
public abstract class BaseMoongateService : IMoongateService
{
    /// <summary>
    /// Starts the service.
    /// </summary>
    /// <returns>A completed task.</returns>
    public Task StartAsync()
        => Task.CompletedTask;

    /// <summary>
    /// Stops the service.
    /// </summary>
    /// <returns>A completed task.</returns>
    public Task StopAsync()
        => Task.CompletedTask;
}
