using Moongate.Server.Interfaces.Services.Console;
using Serilog.Events;

namespace Moongate.Tests.Server.Support;

public sealed class ConsoleUiSerilogSinkTestConsoleUiService : IConsoleUiService
{
    public string LastLine { get; private set; } = string.Empty;

    public LogEventLevel LastLevel { get; private set; }

    public IReadOnlyCollection<string> LastHighlightedValues { get; private set; } = [];

    public bool IsInteractive => true;

    public bool IsInputLocked => false;

    public char UnlockCharacter => '*';

    public void UpdateInput(string input) { }

    public void LockInput() { }

    public void UnlockInput() { }

    public void WriteLogLine(
        string line,
        LogEventLevel level,
        IReadOnlyCollection<string>? highlightedValues = null
    )
    {
        LastLine = line;
        LastLevel = level;
        LastHighlightedValues = highlightedValues ?? [];
    }
}
