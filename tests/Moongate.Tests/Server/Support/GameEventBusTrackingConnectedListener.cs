using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Services;
using Moongate.Server.Interfaces.Services.Events;

namespace Moongate.Tests.Server.Support;

public sealed class GameEventBusTrackingConnectedListener : IGameEventListener<PlayerConnectedEvent>
{
    public List<PlayerConnectedEvent> Received { get; } = [];

    public Task HandleAsync(PlayerConnectedEvent gameEvent, CancellationToken cancellationToken = default)
    {
        Received.Add(gameEvent);

        return Task.CompletedTask;
    }
}
