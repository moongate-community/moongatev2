using Moongate.Server.Data.Events;
using Moongate.Server.Services;
using Moongate.Tests.Server.Support;

namespace Moongate.Tests.Server;

public class GameEventScriptBridgeServiceTests
{
    [Test]
    public async Task HandleAsync_ShouldExecuteScriptCallback_WithSnakeCaseEventName()
    {
        var eventBus = new GameEventScriptBridgeTestGameEventBusService();
        var scriptEngine = new GameEventScriptBridgeTestScriptEngineService();
        var service = new GameEventScriptBridgeService(eventBus, scriptEngine);
        var gameEvent = new PlayerConnectedEvent(42, "127.0.0.1:2593", 100);

        await service.HandleAsync(gameEvent);

        Assert.That(scriptEngine.LastCallbackName, Is.EqualTo("on_player_connected"));
        Assert.That(scriptEngine.LastCallbackArgs, Has.Length.EqualTo(1));
        Assert.That(scriptEngine.LastCallbackArgs![0], Is.EqualTo(gameEvent));
    }

    [Test]
    public async Task StartAsync_ShouldRegisterGlobalGameEventListener()
    {
        var eventBus = new GameEventScriptBridgeTestGameEventBusService();
        var scriptEngine = new GameEventScriptBridgeTestScriptEngineService();
        var service = new GameEventScriptBridgeService(eventBus, scriptEngine);

        await service.StartAsync();

        Assert.That(eventBus.LastRegisteredEventType, Is.EqualTo(typeof(IGameEvent)));
        Assert.That(eventBus.LastRegisteredListener, Is.SameAs(service));
    }

    [Test]
    public void StopAsync_ShouldCompleteWithoutErrors()
    {
        var eventBus = new GameEventScriptBridgeTestGameEventBusService();
        var scriptEngine = new GameEventScriptBridgeTestScriptEngineService();
        var service = new GameEventScriptBridgeService(eventBus, scriptEngine);

        Assert.DoesNotThrowAsync(async () => await service.StopAsync());
    }
}
