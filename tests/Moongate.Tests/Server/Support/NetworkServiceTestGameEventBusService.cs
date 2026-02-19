using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Services;
using Moongate.Server.Interfaces.Services.Events;

namespace Moongate.Tests.Server.Support;

public sealed class NetworkServiceTestGameEventBusService : IGameEventBusService
{
    public List<object> Events { get; } = [];

    public ValueTask PublishAsync<TEvent>(TEvent gameEvent, CancellationToken cancellationToken = default)
        where TEvent : IGameEvent
    {
        Events.Add(gameEvent!);

        return ValueTask.CompletedTask;
    }

    public void RegisterListener<TEvent>(IGameEventListener<TEvent> listener) where TEvent : IGameEvent { }
}
