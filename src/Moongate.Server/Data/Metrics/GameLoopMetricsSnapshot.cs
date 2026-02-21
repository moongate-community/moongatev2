namespace Moongate.Server.Data.Metrics;

/// <summary>
/// Immutable snapshot of game-loop runtime metrics.
/// </summary>
public readonly record struct GameLoopMetricsSnapshot(
    long TickCount,
    TimeSpan Uptime,
    double AverageTickMs,
    double MaxTickMs,
    long IdleSleepCount,
    double AverageWorkUnits,
    int OutboundQueueDepth,
    long OutboundPacketsTotal
);
