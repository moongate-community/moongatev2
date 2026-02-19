using Moongate.Server.Interfaces.Services.Lifecycle;

namespace Moongate.Server.Services.Lifecycle;

/// <summary>
/// Tracks shutdown requests and exposes a shared cancellation token.
/// </summary>
public sealed class ServerLifetimeService : IServerLifetimeService, IDisposable
{
    private readonly CancellationTokenSource _shutdownCancellationTokenSource = new();

    public CancellationToken ShutdownToken => _shutdownCancellationTokenSource.Token;

    public void Dispose()
    {
        _shutdownCancellationTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }

    public void RequestShutdown()
    {
        if (_shutdownCancellationTokenSource.IsCancellationRequested)
        {
            return;
        }

        _shutdownCancellationTokenSource.Cancel();
    }
}
