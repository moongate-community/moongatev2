using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Services;
using Moongate.Server.Services;
using Moongate.Scripting.Data.Scripts;
using Moongate.Scripting.Interfaces;

namespace Moongate.Tests.Server;

public class GameEventScriptBridgeServiceTests
{
    [Test]
    public async Task StartAsync_ShouldRegisterGlobalGameEventListener()
    {
        var eventBus = new TestGameEventBusService();
        var scriptEngine = new TestScriptEngineService();
        var service = new GameEventScriptBridgeService(eventBus, scriptEngine);

        await service.StartAsync();

        Assert.That(eventBus.LastRegisteredEventType, Is.EqualTo(typeof(IGameEvent)));
        Assert.That(eventBus.LastRegisteredListener, Is.SameAs(service));
    }

    [Test]
    public async Task HandleAsync_ShouldExecuteScriptCallback_WithSnakeCaseEventName()
    {
        var eventBus = new TestGameEventBusService();
        var scriptEngine = new TestScriptEngineService();
        var service = new GameEventScriptBridgeService(eventBus, scriptEngine);
        var gameEvent = new PlayerConnectedEvent(42, "127.0.0.1:2593", 100);

        await service.HandleAsync(gameEvent);

        Assert.That(scriptEngine.LastCallbackName, Is.EqualTo("on_player_connected"));
        Assert.That(scriptEngine.LastCallbackArgs, Has.Length.EqualTo(1));
        Assert.That(scriptEngine.LastCallbackArgs![0], Is.EqualTo(gameEvent));
    }

    [Test]
    public void StopAsync_ShouldCompleteWithoutErrors()
    {
        var eventBus = new TestGameEventBusService();
        var scriptEngine = new TestScriptEngineService();
        var service = new GameEventScriptBridgeService(eventBus, scriptEngine);

        Assert.DoesNotThrowAsync(async () => await service.StopAsync());
    }

    private sealed class TestGameEventBusService : IGameEventBusService
    {
        public Type? LastRegisteredEventType { get; private set; }

        public object? LastRegisteredListener { get; private set; }

        public ValueTask PublishAsync<TEvent>(TEvent gameEvent, CancellationToken cancellationToken = default)
            where TEvent : IGameEvent => ValueTask.CompletedTask;

        public void RegisterListener<TEvent>(IGameEventListener<TEvent> listener) where TEvent : IGameEvent
        {
            LastRegisteredEventType = typeof(TEvent);
            LastRegisteredListener = listener;
        }
    }

    private sealed class TestScriptEngineService : IScriptEngineService
    {
        public string? LastCallbackName { get; private set; }

        public object[]? LastCallbackArgs { get; private set; }

        public event IScriptEngineService.LuaFileChangedHandler? FileChanged;

        public event EventHandler<ScriptErrorInfo>? OnScriptError;

        public Task StartAsync() => Task.CompletedTask;

        public Task StopAsync() => Task.CompletedTask;

        public void AddCallback(string name, Action<object[]> callback) { }

        public void AddConstant(string name, object value) { }

        public void AddInitScript(string script) { }

        public void AddManualModuleFunction(string moduleName, string functionName, Action<object[]> callback) { }

        public void AddManualModuleFunction<TInput, TOutput>(
            string moduleName,
            string functionName,
            Func<TInput?, TOutput> callback
        ) { }

        public void AddScriptModule(Type type) { }

        public void ClearScriptCache() { }

        public void ExecuteCallback(string name, params object[] args)
        {
            LastCallbackName = name;
            LastCallbackArgs = args;
        }

        public void CallFunction(string functionName, params object[] args)
        {
            LastCallbackName = functionName;
            LastCallbackArgs = args;
        }

        public void ExecuteEngineReady() { }

        public ScriptResult ExecuteFunction(string command) => new() { Success = true };

        public Task<ScriptResult> ExecuteFunctionAsync(string command)
            => Task.FromResult(new ScriptResult { Success = true });

        public void ExecuteFunctionFromBootstrap(string name) { }

        public void ExecuteScript(string script) { }

        public void ExecuteScriptFile(string scriptFile) { }

        public ScriptExecutionMetrics GetExecutionMetrics() => new();

        public void RegisterGlobal(string name, object value) { }

        public void RegisterGlobalFunction(string name, Delegate func) { }

        public string ToScriptEngineFunctionName(string name) => name;

        public bool UnregisterGlobal(string name) => true;
    }
}
