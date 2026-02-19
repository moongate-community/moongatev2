using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Services;
using Moongate.Server.Interfaces.Services.Events;

namespace Moongate.Tests.Server.Support;

public sealed class GameEventBusTrackingAllEventsListener : IGameEventListener<IGameEvent>
{
    public List<IGameEvent> Received { get; } = [];

    public Task HandleAsync(IGameEvent gameEvent, CancellationToken cancellationToken = default)
    {
        Received.Add(gameEvent);

        return Task.CompletedTask;
    }
}
