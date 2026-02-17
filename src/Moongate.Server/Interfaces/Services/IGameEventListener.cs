using Moongate.Server.Data.Events;

namespace Moongate.Server.Interfaces.Services;

/// <summary>
/// Handles domain events emitted by the game server.
/// </summary>
/// <typeparam name="TEvent">Event type handled by the listener.</typeparam>
public interface IGameEventListener<in TEvent> where TEvent : IGameEvent
{
    /// <summary>
    /// Handles a published event.
    /// </summary>
    /// <param name="gameEvent">Published event payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task HandleAsync(TEvent gameEvent, CancellationToken cancellationToken = default);
}
