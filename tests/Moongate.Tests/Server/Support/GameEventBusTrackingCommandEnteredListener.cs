using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Services;

namespace Moongate.Tests.Server.Support;

public sealed class GameEventBusTrackingCommandEnteredListener : IGameEventListener<CommandEnteredEvent>
{
    public List<CommandEnteredEvent> Received { get; } = [];

    public Task HandleAsync(CommandEnteredEvent gameEvent, CancellationToken cancellationToken = default)
    {
        Received.Add(gameEvent);
        return Task.CompletedTask;
    }
}
