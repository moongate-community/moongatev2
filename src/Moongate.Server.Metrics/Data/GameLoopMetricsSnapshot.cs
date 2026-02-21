using Moongate.Server.Metrics.Data.Attributes;
using Moongate.Server.Metrics.Data.Types;

namespace Moongate.Server.Metrics.Data;

/// <summary>
/// Immutable snapshot of game-loop runtime metrics.
/// </summary>
public sealed class GameLoopMetricsSnapshot
{
    public GameLoopMetricsSnapshot(
        long tickCount,
        TimeSpan uptime,
        double averageTickMs,
        double maxTickMs,
        long idleSleepCount,
        double averageWorkUnits,
        int outboundQueueDepth,
        long outboundPacketsTotal
    )
    {
        TickCount = tickCount;
        Uptime = uptime;
        AverageTickMs = averageTickMs;
        MaxTickMs = maxTickMs;
        IdleSleepCount = idleSleepCount;
        AverageWorkUnits = averageWorkUnits;
        OutboundQueueDepth = outboundQueueDepth;
        OutboundPacketsTotal = outboundPacketsTotal;
    }

    [Metric("ticks.total")]
    public long TickCount { get; }

    [Metric("uptime.ms", Transform = MetricValueTransformType.TimeSpanMilliseconds)]
    public TimeSpan Uptime { get; }

    [Metric("loop.tick.duration.avg_ms", Aliases = new[] { "tick.duration.avg_ms" })]
    public double AverageTickMs { get; }

    [Metric("loop.tick.duration.max_ms")]
    public double MaxTickMs { get; }

    [Metric("loop.idle.sleep.count")]
    public long IdleSleepCount { get; }

    [Metric("loop.work.units.avg")]
    public double AverageWorkUnits { get; }

    [Metric("network.outbound.queue.depth")]
    public int OutboundQueueDepth { get; }

    [Metric("network.outbound.packets.total")]
    public long OutboundPacketsTotal { get; }
}
