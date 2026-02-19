using Moongate.Server.Data.Events;

namespace Moongate.Server.Interfaces.Services.Events;

/// <summary>
/// In-process event bus for publishing and subscribing domain events.
/// </summary>
public interface IGameEventBusService
{
    /// <summary>
    /// Publishes an event to all listeners registered for its type.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <param name="gameEvent">Event payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask PublishAsync<TEvent>(TEvent gameEvent, CancellationToken cancellationToken = default)
        where TEvent : IGameEvent;

    /// <summary>
    /// Registers a listener for the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <param name="listener">Listener instance.</param>
    void RegisterListener<TEvent>(IGameEventListener<TEvent> listener) where TEvent : IGameEvent;
}
