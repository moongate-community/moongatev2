using Moongate.Core.Extensions.Strings;
using Moongate.Scripting.Interfaces;
using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Services.Events;
using Serilog;

namespace Moongate.Server.Services.Events;

public class GameEventScriptBridgeService : IGameEventScriptBridgeService, IGameEventListener<IGameEvent>
{
    private readonly ILogger _logger = Log.ForContext<GameEventScriptBridgeService>();

    private readonly IGameEventBusService _gameEventBusService;
    private readonly IScriptEngineService _scriptEngineService;

    public GameEventScriptBridgeService(IGameEventBusService gameEventBusService, IScriptEngineService scriptEngineService)
    {
        _gameEventBusService = gameEventBusService;
        _scriptEngineService = scriptEngineService;
    }

    public async Task HandleAsync(IGameEvent gameEvent, CancellationToken cancellationToken = default)
    {
        _logger.Debug("Received game event: {EventType}", gameEvent.GetType().Name);
        var scriptFunctionName = GetScriptFunctionNameForEvent(gameEvent);
        _logger.Debug("Looking for script function: {FunctionName}", scriptFunctionName);
        _scriptEngineService.CallFunction(scriptFunctionName, gameEvent);
    }

    public async Task StartAsync()
    {
        _logger.Debug("Subscribing to game events for script bridge...");
        _gameEventBusService.RegisterListener(this);
    }

    public async Task StopAsync() { }

    private static string GetScriptFunctionNameForEvent(IGameEvent gameEvent)
    {
        var eventName = gameEvent.GetType().Name.ToSnakeCase();

        if (eventName.EndsWith("_event", StringComparison.Ordinal))
        {
            eventName = eventName[..^"_event".Length];
        }

        return $"on_{eventName}";
    }
}
