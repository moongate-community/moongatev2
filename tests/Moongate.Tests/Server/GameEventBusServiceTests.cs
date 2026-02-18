using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Services;
using Moongate.Server.Services;

namespace Moongate.Tests.Server;

public class GameEventBusServiceTests
{
    private sealed class TrackingAllEventsListener : IGameEventListener<IGameEvent>
    {
        public List<IGameEvent> Received { get; } = [];

        public Task HandleAsync(IGameEvent gameEvent, CancellationToken cancellationToken = default)
        {
            Received.Add(gameEvent);
            return Task.CompletedTask;
        }
    }

    private sealed class TrackingConnectedListener : IGameEventListener<PlayerConnectedEvent>
    {
        public List<PlayerConnectedEvent> Received { get; } = [];

        public Task HandleAsync(PlayerConnectedEvent gameEvent, CancellationToken cancellationToken = default)
        {
            Received.Add(gameEvent);

            return Task.CompletedTask;
        }
    }

    private sealed class FailingConnectedListener : IGameEventListener<PlayerConnectedEvent>
    {
        public Task HandleAsync(PlayerConnectedEvent gameEvent, CancellationToken cancellationToken = default)
            => Task.FromException(new InvalidOperationException("listener failure"));
    }

    [Test]
    public async Task PublishAsync_ShouldNotifyRegisteredListeners()
    {
        var bus = new GameEventBusService();
        var listener = new TrackingConnectedListener();
        var gameEvent = new PlayerConnectedEvent(42, "127.0.0.1:2593", 123);

        bus.RegisterListener(listener);
        await bus.PublishAsync(gameEvent);

        Assert.That(listener.Received.Count, Is.EqualTo(1));
        Assert.That(listener.Received[0].SessionId, Is.EqualTo(42));
    }

    [Test]
    public async Task PublishAsync_WhenOneListenerFails_ShouldContinueOtherListeners()
    {
        var bus = new GameEventBusService();
        var failing = new FailingConnectedListener();
        var tracking = new TrackingConnectedListener();

        bus.RegisterListener(failing);
        bus.RegisterListener(tracking);

        await bus.PublishAsync(new PlayerConnectedEvent(7, null, 1));

        Assert.That(tracking.Received.Count, Is.EqualTo(1));
        Assert.That(tracking.Received[0].SessionId, Is.EqualTo(7));
    }

    [Test]
    public async Task PublishAsync_WhenGlobalListenerRegistered_ShouldReceiveAllEvents()
    {
        var bus = new GameEventBusService();
        var allEventsListener = new TrackingAllEventsListener();

        bus.RegisterListener<IGameEvent>(allEventsListener);

        var connected = new PlayerConnectedEvent(10, "127.0.0.1:2593", 100);
        var disconnected = new PlayerDisconnectedEvent(10, "127.0.0.1:2593", 101);

        await bus.PublishAsync(connected);
        await bus.PublishAsync(disconnected);

        Assert.That(allEventsListener.Received.Count, Is.EqualTo(2));
        Assert.That(allEventsListener.Received[0], Is.EqualTo(connected));
        Assert.That(allEventsListener.Received[1], Is.EqualTo(disconnected));
    }
}
