using Moongate.Tests.Server.Support;

namespace Moongate.Tests.Server;

public class BaseOutboundEventListenerTests
{
    [Test]
    public async Task HandleAsync_ShouldInvokeHandleCoreAsync()
    {
        var gameEventBus = new BaseOutboundEventListenerTestGameEventBusService();
        var listener = new BaseOutboundEventListenerTestListener(gameEventBus);
        var gameEvent = new BaseOutboundEventListenerTestEvent(7);

        await listener.HandleAsync(gameEvent);

        Assert.That(listener.Received, Is.EqualTo(new[] { gameEvent }));
    }

    [Test]
    public async Task StartAsync_ShouldRegisterListenerOnGameEventBus()
    {
        var gameEventBus = new BaseOutboundEventListenerTestGameEventBusService();
        var listener = new BaseOutboundEventListenerTestListener(gameEventBus);

        await listener.StartAsync();

        Assert.That(gameEventBus.RegisteredListeners, Does.Contain(listener));
    }
}
