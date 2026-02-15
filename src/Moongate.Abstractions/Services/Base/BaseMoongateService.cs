using Moongate.Abstractions.Interfaces.Services;

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
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the service.
    /// </summary>
    /// <returns>A completed task.</returns>
    public Task StopAsync()
    {
        return Task.CompletedTask;
    }
}
