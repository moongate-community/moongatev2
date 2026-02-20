using Moongate.Server.Services.Console.Internal.Logging;
using Moongate.Tests.Server.Support;
using Serilog.Events;
using Serilog.Parsing;
using System.Globalization;

namespace Moongate.Tests.Server;

public sealed class ConsoleUiSerilogSinkTests
{
    [Test]
    public void Emit_ShouldHighlightNamedPairs_AndAvoidRawScalarHighlights()
    {
        var consoleUiService = new ConsoleUiSerilogSinkTestConsoleUiService();
        var sink = new ConsoleUiSerilogSink(consoleUiService);
        var parser = new MessageTemplateParser();
        var template = parser.Parse(
            "Outbound packet Session={Session} OpCode={OpCode} Name={Name} Length={Length}"
        );

        var logEvent = new LogEvent(
            DateTimeOffset.Parse("2026-02-19T17:55:06Z", CultureInfo.InvariantCulture),
            LogEventLevel.Information,
            exception: null,
            template,
            [
                new("Session", new ScalarValue(2)),
                new("OpCode", new ScalarValue("0xB9")),
                new("Name", new ScalarValue("SupportFeaturesPacket")),
                new("Length", new ScalarValue(5))
            ]
        );

        sink.Emit(logEvent);

        Assert.That(consoleUiService.LastLine, Does.Contain("Length=5"));
        Assert.That(consoleUiService.LastHighlightedValues, Does.Contain("Session=2"));
        Assert.That(consoleUiService.LastHighlightedValues, Does.Contain("Length=5"));
        Assert.That(consoleUiService.LastHighlightedValues, Does.Not.Contain("5"));
    }
}
