using System.Globalization;
using Moongate.Server.Interfaces.Services.Console;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace Moongate.Server.Services.Console.Internal.Logging;

/// <summary>
/// Writes Serilog events to the managed console UI renderer.
/// </summary>
public sealed class ConsoleUiSerilogSink : ILogEventSink
{
    private const string OutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

    private readonly IConsoleUiService _consoleUiService;
    private readonly MessageTemplateTextFormatter _formatter = new(OutputTemplate, CultureInfo.InvariantCulture);

    public ConsoleUiSerilogSink(IConsoleUiService consoleUiService)
        => _consoleUiService = consoleUiService;

    public void Emit(LogEvent logEvent)
    {
        ArgumentNullException.ThrowIfNull(logEvent);

        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        _formatter.Format(logEvent, writer);
        _consoleUiService.WriteLogLine(
            writer.ToString(),
            logEvent.Level,
            GetHighlightedValues(logEvent.Properties)
        );
    }

    private static HashSet<string> GetHighlightedValues(IReadOnlyDictionary<string, LogEventPropertyValue> properties)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);

        foreach (var pair in properties)
        {
            var propertyName = pair.Key;
            var value = pair.Value;
            var rendered = value.ToString();

            if (string.IsNullOrWhiteSpace(rendered))
            {
                continue;
            }

            result.Add($"{propertyName}={rendered}");

            // Strings may be quoted in property rendering while the message prints them raw.
            if (rendered.Length >= 2 && rendered[0] == '"' && rendered[^1] == '"')
            {
                result.Add($"{propertyName}={rendered[1..^1]}");
            }
        }

        return result;
    }
}
