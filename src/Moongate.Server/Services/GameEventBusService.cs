using System.Collections.Concurrent;
using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Services;
using Serilog;

namespace Moongate.Server.Services;

public sealed class GameEventBusService : IGameEventBusService
{
    private readonly ConcurrentDictionary<Type, List<object>> _listeners = new();
    private readonly ILogger _logger = Log.ForContext<GameEventBusService>();

    public async ValueTask PublishAsync<TEvent>(TEvent gameEvent, CancellationToken cancellationToken = default)
        where TEvent : IGameEvent
    {
        if (!_listeners.TryGetValue(typeof(TEvent), out var listeners))
        {
            return;
        }

        object[] snapshot;

        lock (listeners)
        {
            if (listeners.Count == 0)
            {
                return;
            }

            snapshot = listeners.ToArray();
        }

        foreach (var listenerObject in snapshot)
        {
            if (listenerObject is not IGameEventListener<TEvent> listener)
            {
                continue;
            }

            try
            {
                await listener.HandleAsync(gameEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Game event listener failed for event type {EventType}", typeof(TEvent).Name);
            }
        }
    }

    public void RegisterListener<TEvent>(IGameEventListener<TEvent> listener) where TEvent : IGameEvent
    {
        var listeners = _listeners.GetOrAdd(typeof(TEvent), static _ => []);

        lock (listeners)
        {
            listeners.Add(listener);
        }
    }
}
