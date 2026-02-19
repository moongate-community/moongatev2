namespace Moongate.Server.Data.Internal.Console;

internal readonly record struct ConsoleLogLine(IReadOnlyList<ConsoleSegment>? Segments);
