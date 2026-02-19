using Moongate.Server.Data.Metrics;
using Moongate.Server.Interfaces.Services.Metrics;

namespace Moongate.Tests.Server.Support;

public sealed class MetricsProvidersTestGameLoopService : IGameLoopMetricsSource
{
    public long TickCount { get; set; }

    public TimeSpan Uptime { get; set; }

    public double AverageTickMs { get; set; }

    public GameLoopMetricsSnapshot GetMetricsSnapshot()
        => new(TickCount, Uptime, AverageTickMs);
}
