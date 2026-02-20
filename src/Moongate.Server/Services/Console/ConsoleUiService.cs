using Moongate.Server.Data.Internal.Console;
using Moongate.Server.Interfaces.Services.Console;
using Serilog.Events;
using Spectre.Console;

namespace Moongate.Server.Services.Console;

/// <summary>
/// Renders server logs and a fixed command prompt in the terminal.
/// </summary>
public sealed class ConsoleUiService : IConsoleUiService
{
    private const string PromptPrefix = "moongate> ";
    private const string LockedPromptPrefix = "moongate [LOCKED]> ";
    private const char PromptUnlockCharacter = '*';
    private static readonly Style PropertyStyle = new(Color.Aqua);

    private readonly Lock _sync = new();

    private string _input = string.Empty;

    public ConsoleUiService()
    {
        IsInteractive = IsInteractiveConsole();
        IsInputLocked = true;
    }

    public bool IsInteractive { get; private set; }

    public bool IsInputLocked { get; private set; }

    public char UnlockCharacter => PromptUnlockCharacter;

    public void LockInput()
    {
        if (!IsInteractive)
        {
            return;
        }

        lock (_sync)
        {
            IsInputLocked = true;
            _input = string.Empty;
            RenderPromptUnsafe();
        }
    }

    public void UnlockInput()
    {
        if (!IsInteractive)
        {
            return;
        }

        lock (_sync)
        {
            IsInputLocked = false;
            RenderPromptUnsafe();
        }
    }

    public void UpdateInput(string input)
    {
        if (!IsInteractive)
        {
            return;
        }

        lock (_sync)
        {
            _input = input;
            RenderPromptUnsafe();
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
            System.Console.WriteLine(line);

            return;
        }

        lock (_sync)
        {
            try
            {
                ClearPromptRowUnsafe();

                foreach (var item in SplitLines(line))
                {
                    WriteFormattedLineUnsafe(item, level, highlightedValues);
                }

                RenderPromptUnsafe();
            }
            catch (IOException)
            {
                IsInteractive = false;
                System.Console.WriteLine(line);
            }
            catch (ArgumentOutOfRangeException)
            {
                IsInteractive = false;
                System.Console.WriteLine(line);
            }
        }
    }

    private void ClearPromptRowUnsafe()
    {
        var width = Math.Max(1, System.Console.WindowWidth);
        var promptRow = GetPromptRowUnsafe();

        System.Console.SetCursorPosition(0, promptRow);
        System.Console.Write(new string(' ', width));
        System.Console.SetCursorPosition(0, promptRow);
    }

    private static IReadOnlyList<ConsoleSegment> CreateSegments(
        string line,
        LogEventLevel level,
        IReadOnlyCollection<string>? highlightedValues
    )
    {
        var baseStyle = GetStyle(level);

        if (string.IsNullOrEmpty(line))
        {
            return [];
        }

        if (highlightedValues is null || highlightedValues.Count == 0)
        {
            return [new(line, baseStyle)];
        }

        var orderedHighlights = highlightedValues
                                .Where(value => !string.IsNullOrEmpty(value))
                                .Distinct(StringComparer.Ordinal)
                                .OrderByDescending(value => value.Length)
                                .ToArray();

        if (orderedHighlights.Length == 0)
        {
            return [new(line, baseStyle)];
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

        return segments;
    }

    private static int GetPromptRowUnsafe()
    {
        var bufferHeight = Math.Max(1, System.Console.BufferHeight);
        var windowHeight = Math.Max(1, System.Console.WindowHeight);
        var row = System.Console.WindowTop + windowHeight - 1;

        return Math.Clamp(row, 0, bufferHeight - 1);
    }

    private static Style GetStyle(LogEventLevel level)
        => level switch
        {
            LogEventLevel.Verbose     => new(Color.Grey),
            LogEventLevel.Debug       => new(Color.Grey),
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

        if (System.Console.IsInputRedirected || System.Console.IsOutputRedirected)
        {
            return false;
        }

        return true;
    }

    private void RenderPromptUnsafe()
    {
        var width = Math.Max(1, System.Console.WindowWidth);
        var promptRow = GetPromptRowUnsafe();
        var promptPrefix = IsInputLocked ? LockedPromptPrefix : PromptPrefix;

        System.Console.SetCursorPosition(0, promptRow);
        System.Console.Write(new string(' ', width));
        System.Console.SetCursorPosition(0, promptRow);

        var line = promptPrefix + _input;
        var visible = line.Length > width ? line[..width] : line;
        AnsiConsole.Console.Write(new Text(visible, new(Color.Grey)));

        var cursorColumn = Math.Min(width - 1, promptPrefix.Length + _input.Length);
        System.Console.SetCursorPosition(cursorColumn, promptRow);
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

    private static void WriteFormattedLineUnsafe(
        string line,
        LogEventLevel level,
        IReadOnlyCollection<string>? highlightedValues
    )
    {
        var segments = CreateSegments(line, level, highlightedValues);

        if (segments.Count == 0)
        {
            System.Console.WriteLine();

            return;
        }

        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];

            if (segment.Text.Length == 0)
            {
                continue;
            }

            AnsiConsole.Console.Write(new Text(segment.Text, segment.Style));
        }

        System.Console.WriteLine();
    }
}
