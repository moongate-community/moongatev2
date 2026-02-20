using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Services.Events;

namespace Moongate.Tests.Server.Support;

public sealed class BaseOutboundEventListenerTestGameEventBusService : IGameEventBusService
{
    public List<object> RegisteredListeners { get; } = [];

    public async ValueTask PublishAsync<TEvent>(TEvent gameEvent, CancellationToken cancellationToken = default)
        where TEvent : IGameEvent
    {
        foreach (var listener in RegisteredListeners)
        {
            if (listener is IGameEventListener<TEvent> typed)
            {
                await typed.HandleAsync(gameEvent, cancellationToken);
            }
        }
    }

    public void RegisterListener<TEvent>(IGameEventListener<TEvent> listener) where TEvent : IGameEvent
        => RegisteredListeners.Add(listener);
}
