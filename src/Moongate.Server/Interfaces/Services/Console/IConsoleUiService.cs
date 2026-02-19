using Serilog.Events;

namespace Moongate.Server.Interfaces.Services.Console;

/// <summary>
/// Manages console rendering for log lines and interactive command prompt input.
/// </summary>
public interface IConsoleUiService
{
    /// <summary>
    /// Gets whether interactive prompt rendering is currently enabled.
    /// </summary>
    bool IsInteractive { get; }

    /// <summary>
    /// Gets whether console input is currently locked.
    /// </summary>
    bool IsInputLocked { get; }

    /// <summary>
    /// Gets the character used to unlock console input while locked.
    /// </summary>
    char UnlockCharacter { get; }

    /// <summary>
    /// Locks console input.
    /// </summary>
    void LockInput();

    /// <summary>
    /// Unlocks console input.
    /// </summary>
    void UnlockInput();

    /// <summary>
    /// Updates the command buffer shown in the prompt row.
    /// </summary>
    /// <param name="input">Current command text.</param>
    void UpdateInput(string input);

    /// <summary>
    /// Scrolls the log viewport up by one page.
    /// </summary>
    void ScrollPageUp();

    /// <summary>
    /// Scrolls the log viewport down by one page.
    /// </summary>
    void ScrollPageDown();

    /// <summary>
    /// Scrolls the log viewport to the oldest available line.
    /// </summary>
    void ScrollToTop();

    /// <summary>
    /// Scrolls the log viewport to the most recent line.
    /// </summary>
    void ScrollToBottom();

    /// <summary>
    /// Writes one log line to the console output area.
    /// </summary>
    /// <param name="line">Formatted log line.</param>
    /// <param name="level">Log level used for row color styling.</param>
    /// <param name="highlightedValues">Rendered property values to highlight with a dedicated style.</param>
    void WriteLogLine(
        string line,
        LogEventLevel level,
        IReadOnlyCollection<string>? highlightedValues = null
    );
}
