using Moongate.Server.Interfaces.Services;
using Serilog.Events;
using Spectre.Console;

namespace Moongate.Server.Services;

/// <summary>
/// Renders server logs and a fixed command prompt in the terminal.
/// </summary>
public sealed class ConsoleUiService : IConsoleUiService
{
    private const string PromptPrefix = "moongate> ";
    private static readonly Style PropertyStyle = new(Color.Aqua);

    private readonly Lock _sync = new();
    private readonly List<ConsoleLogLine> _logBuffer = [];

    private string _input = string.Empty;

    public ConsoleUiService()
        => IsInteractive = IsInteractiveConsole();

    public bool IsInteractive { get; private set; }

    private readonly record struct ConsoleSegment(string Text, Style Style);

    private readonly record struct ConsoleLogLine(IReadOnlyList<ConsoleSegment>? Segments);

    public void UpdateInput(string input)
    {
        if (!IsInteractive)
        {
            return;
        }

        lock (_sync)
        {
            _input = input;
            RenderUnsafe();
        }
    }

    public void WriteLogLine(
        string line,
        LogEventLevel level,
        IReadOnlyCollection<string>? highlightedValues = null
    )
    {
        if (!IsInteractive)
        {
            Console.WriteLine(line);

            return;
        }

        lock (_sync)
        {
            foreach (var item in SplitLines(line))
            {
                _logBuffer.Add(CreateConsoleLogLine(item, level, highlightedValues));
            }

            TrimBufferForCurrentWindow();
            RenderUnsafe();
        }
    }

    private static ConsoleLogLine CreateConsoleLogLine(
        string line,
        LogEventLevel level,
        IReadOnlyCollection<string>? highlightedValues
    )
    {
        var baseStyle = GetStyle(level);

        if (string.IsNullOrEmpty(line))
        {
            return new([]);
        }

        if (highlightedValues is null || highlightedValues.Count == 0)
        {
            return new([new(line, baseStyle)]);
        }

        var orderedHighlights = highlightedValues
                                .Where(value => !string.IsNullOrEmpty(value))
                                .Distinct(StringComparer.Ordinal)
                                .OrderByDescending(value => value.Length)
                                .ToArray();

        if (orderedHighlights.Length == 0)
        {
            return new([new(line, baseStyle)]);
        }

        var segments = new List<ConsoleSegment>();
        var index = 0;

        while (index < line.Length)
        {
            string? matched = null;

            for (var i = 0; i < orderedHighlights.Length; i++)
            {
                var candidate = orderedHighlights[i];

                if (
                    candidate.Length == 0 ||
                    index + candidate.Length > line.Length ||
                    !line.AsSpan(index, candidate.Length).SequenceEqual(candidate.AsSpan())
                )
                {
                    continue;
                }

                matched = candidate;

                break;
            }

            if (matched is not null)
            {
                segments.Add(new(matched, PropertyStyle));
                index += matched.Length;

                continue;
            }

            var start = index;
            index++;

            while (index < line.Length)
            {
                var foundMatch = false;

                for (var i = 0; i < orderedHighlights.Length; i++)
                {
                    var candidate = orderedHighlights[i];

                    if (
                        candidate.Length > 0 &&
                        index + candidate.Length <= line.Length &&
                        line.AsSpan(index, candidate.Length).SequenceEqual(candidate.AsSpan())
                    )
                    {
                        foundMatch = true;

                        break;
                    }
                }

                if (foundMatch)
                {
                    break;
                }

                index++;
            }

            segments.Add(new(line[start..index], baseStyle));
        }

        return new(segments);
    }

    private static Style GetStyle(LogEventLevel level)
        => level switch
        {
            LogEventLevel.Verbose     => new(Color.Grey),
            LogEventLevel.Debug       => new(Color.Silver),
            LogEventLevel.Information => new(Color.White),
            LogEventLevel.Warning     => new(Color.Yellow),
            LogEventLevel.Error       => new(Color.Red),
            LogEventLevel.Fatal       => new(Color.White, Color.DarkRed),
            _                         => new(Color.White)
        };

    private static bool IsInteractiveConsole()
    {
        if (!Environment.UserInteractive)
        {
            return false;
        }

        if (Console.IsInputRedirected || Console.IsOutputRedirected)
        {
            return false;
        }

        return true;
    }

    private void RenderUnsafe()
    {
        try
        {
            var width = Math.Max(1, Console.WindowWidth);
            var promptRow = Math.Max(0, Console.WindowHeight - 1);

            TrimBuffer(promptRow);
            var startIndex = Math.Max(0, _logBuffer.Count - promptRow);

            for (var row = 0; row < promptRow; row++)
            {
                var line = startIndex + row < _logBuffer.Count ? _logBuffer[startIndex + row] : default;
                WriteRow(line, row, width);
            }

            WritePromptRow(PromptPrefix + _input, promptRow, width);
            var cursorColumn = Math.Min(width - 1, PromptPrefix.Length + _input.Length);
            Console.SetCursorPosition(cursorColumn, promptRow);
        }
        catch (IOException)
        {
            IsInteractive = false;
        }
        catch (ArgumentOutOfRangeException)
        {
            IsInteractive = false;
        }
    }

    private static IEnumerable<string> SplitLines(string line)
    {
        if (string.IsNullOrEmpty(line))
        {
            yield return string.Empty;

            yield break;
        }

        var split = line.Replace("\r", string.Empty, StringComparison.Ordinal)
                        .Split('\n');

        var length = split.Length;

        if (length > 0 && split[length - 1].Length == 0)
        {
            length--;
        }

        for (var i = 0; i < length; i++)
        {
            yield return split[i];
        }
    }

    private void TrimBuffer(int maxRows)
    {
        while (_logBuffer.Count > maxRows)
        {
            _logBuffer.RemoveAt(0);
        }
    }

    private void TrimBufferForCurrentWindow()
    {
        var logRows = Math.Max(0, Console.WindowHeight - 1);
        TrimBuffer(logRows);
    }

    private static void WritePromptRow(string line, int row, int width)
    {
        Console.SetCursorPosition(0, row);
        Console.Write(new string(' ', width));
        Console.SetCursorPosition(0, row);
        var visible = line.Length > width ? line[..width] : line;
        AnsiConsole.Console.Write(new Text(visible, new(Color.Grey)));
    }

    private static void WriteRow(ConsoleLogLine line, int row, int width)
    {
        Console.SetCursorPosition(0, row);
        Console.Write(new string(' ', width));
        Console.SetCursorPosition(0, row);

        if (line.Segments is null || line.Segments.Count == 0)
        {
            return;
        }

        var remaining = width;

        foreach (var segment in line.Segments)
        {
            if (remaining <= 0)
            {
                break;
            }

            if (segment.Text.Length == 0)
            {
                continue;
            }

            var visible = segment.Text.Length > remaining ? segment.Text[..remaining] : segment.Text;
            AnsiConsole.Console.Write(new Text(visible, segment.Style));
            remaining -= visible.Length;
        }
    }
}
