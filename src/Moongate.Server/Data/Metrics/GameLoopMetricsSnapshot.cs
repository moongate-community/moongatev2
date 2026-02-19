namespace Moongate.Server.Data.Metrics;

/// <summary>
/// Immutable snapshot of game-loop runtime metrics.
/// </summary>
public readonly record struct GameLoopMetricsSnapshot(
    long TickCount,
    TimeSpan Uptime,
    double AverageTickMs
);
