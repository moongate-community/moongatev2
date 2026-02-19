namespace Moongate.Server.Data.Metrics;

/// <summary>
/// Immutable snapshot of timer-wheel runtime metrics.
/// </summary>
public readonly record struct TimerMetricsSnapshot(
    int ActiveTimerCount,
    long TotalRegisteredTimers,
    long TotalExecutedCallbacks,
    long CallbackErrors,
    double AverageCallbackDurationMs
);
