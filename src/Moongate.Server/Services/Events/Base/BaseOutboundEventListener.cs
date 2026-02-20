using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Services.Events;

namespace Moongate.Server.Services.Events.Base;

/// <summary>
/// Base implementation for outbound event listeners that auto-register on the game event bus.
/// </summary>
/// <typeparam name="TEvent">Domain event type handled by this listener.</typeparam>
public abstract class BaseOutboundEventListener<TEvent> : IOutboundEventListener<TEvent>
    where TEvent : IGameEvent
{
    private readonly IGameEventBusService _gameEventBusService;

    protected BaseOutboundEventListener(IGameEventBusService gameEventBusService)
        => _gameEventBusService = gameEventBusService;

    /// <inheritdoc />
    public Task HandleAsync(TEvent gameEvent, CancellationToken cancellationToken = default)
        => HandleCoreAsync(gameEvent, cancellationToken);

    /// <inheritdoc />
    public virtual Task StartAsync()
    {
        _gameEventBusService.RegisterListener(this);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task StopAsync()
        => Task.CompletedTask;

    /// <summary>
    /// Handles a single published domain event.
    /// </summary>
    /// <param name="gameEvent">Published event payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    protected abstract Task HandleCoreAsync(TEvent gameEvent, CancellationToken cancellationToken);
}
