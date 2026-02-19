namespace Moongate.Server.Data.Metrics;

/// <summary>
/// Immutable snapshot of network parser/runtime metrics.
/// </summary>
public readonly record struct NetworkMetricsSnapshot(
    int ActiveSessionCount,
    long TotalReceivedBytes,
    int TotalParsedPackets,
    int TotalParserErrors
);
