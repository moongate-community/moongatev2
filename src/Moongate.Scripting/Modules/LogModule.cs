using Moongate.Scripting.Attributes.Scripts;
using Serilog;
using Serilog.Events;

namespace Moongate.Scripting.Modules;

[ScriptModule("log", "Provides logging functionalities to scripts.")]
public class LogModule
{
    private readonly ILogger _logger = Log.ForContext<LogModule>();

    [ScriptFunction(helpText: "Logs a message at the ERROR level.")]
    public void Error(string message, params object[]? args)
    {
        Write(LogEventLevel.Error, message, args);
    }

    [ScriptFunction(helpText: "Logs a message at the INFO level.")]
    public void Info(string message, params object[]? args)
    {
        Write(LogEventLevel.Information, message, args);
    }

    [ScriptFunction(helpText: "Logs a message at the WARNING level.")]
    public void Warning(string message, params object[]? args)
    {
        Write(LogEventLevel.Warning, message, args);
    }

    private void Write(LogEventLevel level, string message, object[]? args)
    {
        if (args is { Length: > 0 })
        {
            _logger.Write(level, "{LogMessage} | {@LogArgs}", message, args);
            return;
        }

        _logger.Write(level, "{LogMessage}", message);
    }
}
