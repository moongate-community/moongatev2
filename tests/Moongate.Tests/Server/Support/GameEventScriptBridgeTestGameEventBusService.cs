using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Services;

namespace Moongate.Tests.Server.Support;

public sealed class GameEventScriptBridgeTestGameEventBusService : IGameEventBusService
{
    public Type? LastRegisteredEventType { get; private set; }
    public object? LastRegisteredListener { get; private set; }

    public ValueTask PublishAsync<TEvent>(TEvent gameEvent, CancellationToken cancellationToken = default)
        where TEvent : IGameEvent
        => ValueTask.CompletedTask;

    public void RegisterListener<TEvent>(IGameEventListener<TEvent> listener) where TEvent : IGameEvent
    {
        LastRegisteredEventType = typeof(TEvent);
        LastRegisteredListener = listener;
    }
}
