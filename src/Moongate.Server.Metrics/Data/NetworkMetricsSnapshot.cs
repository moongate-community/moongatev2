using Moongate.Server.Metrics.Data.Attributes;

namespace Moongate.Server.Metrics.Data;

/// <summary>
/// Immutable snapshot of network parser/runtime metrics.
/// </summary>
public sealed class NetworkMetricsSnapshot
{
    public NetworkMetricsSnapshot(
        int activeSessionCount,
        long totalReceivedBytes,
        int totalParsedPackets,
        int totalParserErrors,
        int inboundQueueDepth,
        int totalUnknownOpcodeDrops
    )
    {
        ActiveSessionCount = activeSessionCount;
        TotalReceivedBytes = totalReceivedBytes;
        TotalParsedPackets = totalParsedPackets;
        TotalParserErrors = totalParserErrors;
        InboundQueueDepth = inboundQueueDepth;
        TotalUnknownOpcodeDrops = totalUnknownOpcodeDrops;
    }

    [Metric("sessions.active")]
    public int ActiveSessionCount { get; }

    [Metric("bytes.received.total")]
    public long TotalReceivedBytes { get; }

    [Metric("network.inbound.packets.total", Aliases = new[] { "packets.parsed.total" })]
    public int TotalParsedPackets { get; }

    [Metric("parser.errors.total")]
    public int TotalParserErrors { get; }

    [Metric("network.inbound.queue.depth")]
    public int InboundQueueDepth { get; }

    [Metric("network.inbound.unknown_opcode.total")]
    public int TotalUnknownOpcodeDrops { get; }
}
