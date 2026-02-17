using DarkLilly.Scripting.Lua.Attributes.Scripts;
using Serilog;

namespace DarkLilly.Scripting.Lua.Modules;

[ScriptModule("log", "Provides logging functionalities to scripts.")]
public class LogModule
{
    private readonly ILogger _logger = Log.ForContext<LogModule>();

    [ScriptFunction(helpText: "Logs a message at the ERROR level.")]
    public void Error(string message, params object[]? args)
    {
        _logger.Error(message, args);
    }

    [ScriptFunction(helpText: "Logs a message at the INFO level.")]
    public void Info(string message, params object[]? args)
    {
        _logger.Information(message, args);
    }

    [ScriptFunction(helpText: "Logs a message at the WARNING level.")]
    public void Warning(string message, params object[]? args)
    {
        _logger.Warning(message, args);
    }
}
