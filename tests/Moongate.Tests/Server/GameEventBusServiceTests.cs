using Moongate.Server.Data.Events;
using Moongate.Server.Services;
using Moongate.Tests.Server.Support;

namespace Moongate.Tests.Server;

public class GameEventBusServiceTests
{
    [Test]
    public async Task PublishAsync_ShouldNotifyRegisteredListeners()
    {
        var bus = new GameEventBusService();
        var listener = new GameEventBusTrackingConnectedListener();
        var gameEvent = new PlayerConnectedEvent(42, "127.0.0.1:2593", 123);

        bus.RegisterListener(listener);
        await bus.PublishAsync(gameEvent);

        Assert.That(listener.Received.Count, Is.EqualTo(1));
        Assert.That(listener.Received[0].SessionId, Is.EqualTo(42));
    }

    [Test]
    public async Task PublishAsync_WhenGlobalListenerRegistered_ShouldReceiveAllEvents()
    {
        var bus = new GameEventBusService();
        var allEventsListener = new GameEventBusTrackingAllEventsListener();

        bus.RegisterListener(allEventsListener);

        var connected = new PlayerConnectedEvent(10, "127.0.0.1:2593", 100);
        var disconnected = new PlayerDisconnectedEvent(10, "127.0.0.1:2593", 101);

        await bus.PublishAsync(connected);
        await bus.PublishAsync(disconnected);

        Assert.That(allEventsListener.Received.Count, Is.EqualTo(2));
        Assert.That(allEventsListener.Received[0], Is.EqualTo(connected));
        Assert.That(allEventsListener.Received[1], Is.EqualTo(disconnected));
    }

    [Test]
    public async Task PublishAsync_WhenOneListenerFails_ShouldContinueOtherListeners()
    {
        var bus = new GameEventBusService();
        var failing = new GameEventBusFailingConnectedListener();
        var tracking = new GameEventBusTrackingConnectedListener();

        bus.RegisterListener(failing);
        bus.RegisterListener(tracking);

        await bus.PublishAsync(new PlayerConnectedEvent(7, null, 1));

        Assert.That(tracking.Received.Count, Is.EqualTo(1));
        Assert.That(tracking.Received[0].SessionId, Is.EqualTo(7));
    }
}
