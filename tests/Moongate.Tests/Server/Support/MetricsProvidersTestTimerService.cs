using Moongate.Server.Data.Metrics;
using Moongate.Server.Interfaces.Services.Metrics;

namespace Moongate.Tests.Server.Support;

public sealed class MetricsProvidersTestTimerService : ITimerMetricsSource
{
    public int ActiveTimerCount { get; set; }

    public long TotalRegisteredTimers { get; set; }

    public long TotalExecutedCallbacks { get; set; }

    public long CallbackErrors { get; set; }

    public double AverageCallbackDurationMs { get; set; }

    public long TotalProcessedTicks { get; set; }

    public TimerMetricsSnapshot GetMetricsSnapshot()
        => new(
            ActiveTimerCount,
            TotalRegisteredTimers,
            TotalExecutedCallbacks,
            CallbackErrors,
            AverageCallbackDurationMs,
            TotalProcessedTicks
        );
}
