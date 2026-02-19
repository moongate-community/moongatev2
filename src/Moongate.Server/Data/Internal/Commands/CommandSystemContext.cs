using Serilog.Events;

namespace Moongate.Server.Data.Internal.Commands;

/// <summary>
/// Carries parsed command metadata and output callback for command handlers.
/// </summary>
public sealed class CommandSystemContext
{
    private readonly Action<string, LogEventLevel> _printAction;

    public string CommandText { get; }

    public string[] Arguments { get; }

    public CommandSystemContext(
        string commandText,
        string[] arguments,
        Action<string, LogEventLevel> printAction
    )
    {
        CommandText = commandText;
        Arguments = arguments;
        _printAction = printAction;
    }

    public void Print(string message, params object[] args)
    {
        var formatted = args.Length == 0 ? message : string.Format(message, args);
        _printAction(formatted, LogEventLevel.Information);
    }

    public void PrintError(string message, params object[] args)
    {
        var formatted = args.Length == 0 ? message : string.Format(message, args);
        _printAction(formatted, LogEventLevel.Error);
    }
}
