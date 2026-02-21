using Moongate.Server.Metrics.Data;
using Moongate.Server.Interfaces.Services.Metrics;

namespace Moongate.Tests.Server.Support;

public sealed class MetricsProvidersTestGameLoopService : IGameLoopMetricsSource
{
    public long TickCount { get; set; }

    public TimeSpan Uptime { get; set; }

    public double AverageTickMs { get; set; }

    public double MaxTickMs { get; set; }

    public long IdleSleepCount { get; set; }

    public double AverageWorkUnits { get; set; }

    public int OutboundQueueDepth { get; set; }

    public long OutboundPacketsTotal { get; set; }

    public GameLoopMetricsSnapshot GetMetricsSnapshot()
        => new(
            TickCount,
            Uptime,
            AverageTickMs,
            MaxTickMs,
            IdleSleepCount,
            AverageWorkUnits,
            OutboundQueueDepth,
            OutboundPacketsTotal
        );
}
