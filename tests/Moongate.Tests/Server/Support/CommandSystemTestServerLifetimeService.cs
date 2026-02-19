using Moongate.Server.Interfaces.Services.Lifecycle;

namespace Moongate.Tests.Server.Support;

public sealed class CommandSystemTestServerLifetimeService : IServerLifetimeService
{
    public CancellationToken ShutdownToken => CancellationToken.None;

    public bool IsShutdownRequested { get; private set; }

    public void RequestShutdown()
    {
        IsShutdownRequested = true;
    }
}
