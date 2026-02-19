using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Services;
using Moongate.Server.Interfaces.Services.Events;

namespace Moongate.Tests.Server.Support;

public sealed class GameEventBusFailingConnectedListener : IGameEventListener<PlayerConnectedEvent>
{
    public Task HandleAsync(PlayerConnectedEvent gameEvent, CancellationToken cancellationToken = default)
        => Task.FromException(new InvalidOperationException("listener failure"));
}
