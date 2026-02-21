using Moongate.Server.Metrics.Data.Attributes;

namespace Moongate.Server.Metrics.Data;

/// <summary>
/// Immutable snapshot of timer-wheel runtime metrics.
/// </summary>
public sealed class TimerMetricsSnapshot
{
    public TimerMetricsSnapshot(
        int activeTimerCount,
        long totalRegisteredTimers,
        long totalExecutedCallbacks,
        long callbackErrors,
        double averageCallbackDurationMs,
        long totalProcessedTicks
    )
    {
        ActiveTimerCount = activeTimerCount;
        TotalRegisteredTimers = totalRegisteredTimers;
        TotalExecutedCallbacks = totalExecutedCallbacks;
        CallbackErrors = callbackErrors;
        AverageCallbackDurationMs = averageCallbackDurationMs;
        TotalProcessedTicks = totalProcessedTicks;
    }

    [Metric("active.count")]
    public int ActiveTimerCount { get; }

    [Metric("registered.total")]
    public long TotalRegisteredTimers { get; }

    [Metric("callbacks.executed.total")]
    public long TotalExecutedCallbacks { get; }

    [Metric("callbacks.errors.total")]
    public long CallbackErrors { get; }

    [Metric("callback.duration.avg_ms")]
    public double AverageCallbackDurationMs { get; }

    [Metric("timer.processed_ticks.total")]
    public long TotalProcessedTicks { get; }
}
