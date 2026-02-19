using Moongate.Server.Interfaces.Services.Console;
using Serilog.Events;

namespace Moongate.Tests.Server.Support;

public sealed class CommandSystemTestConsoleUiService : IConsoleUiService
{
    public List<(string Message, LogEventLevel Level)> Lines { get; } = [];

    public bool IsInteractive => true;

    public bool IsInputLocked { get; private set; }

    public char UnlockCharacter => '*';

    public void UpdateInput(string input) { }

    public void LockInput()
    {
        IsInputLocked = true;
    }

    public void UnlockInput()
    {
        IsInputLocked = false;
    }

    public void WriteLogLine(
        string line,
        LogEventLevel level,
        IReadOnlyCollection<string>? highlightedValues = null
    )
    {
        Lines.Add((line, level));
    }
}
