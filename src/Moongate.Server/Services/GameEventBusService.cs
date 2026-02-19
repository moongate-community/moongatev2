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
        var listeners = GetListenersSnapshot<TEvent>();

        if (listeners.Length == 0)
        {
            return;
        }

        foreach (var listenerObject in listeners)
        {
            try
            {
                if (listenerObject is IGameEventListener<TEvent> typedListener)
                {
                    await typedListener.HandleAsync(gameEvent, cancellationToken);

                    continue;
                }

                if (listenerObject is IGameEventListener<IGameEvent> globalListener)
                {
                    await globalListener.HandleAsync(gameEvent, cancellationToken);
                }
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

    private object[] CopyListeners(Type eventType)
    {
        if (!_listeners.TryGetValue(eventType, out var listeners))
        {
            return [];
        }

        lock (listeners)
        {
            return listeners.Count == 0 ? [] : listeners.ToArray();
        }
    }

    private object[] GetListenersSnapshot<TEvent>() where TEvent : IGameEvent
    {
        var typedListeners = CopyListeners(typeof(TEvent));

        if (typeof(TEvent) == typeof(IGameEvent))
        {
            return typedListeners;
        }

        var globalListeners = CopyListeners(typeof(IGameEvent));

        if (typedListeners.Length == 0)
        {
            return globalListeners;
        }

        if (globalListeners.Length == 0)
        {
            return typedListeners;
        }

        var snapshot = new object[typedListeners.Length + globalListeners.Length];
        typedListeners.CopyTo(snapshot, 0);
        globalListeners.CopyTo(snapshot, typedListeners.Length);

        return snapshot;
    }
}
