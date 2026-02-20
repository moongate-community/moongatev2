using Moongate.Server.Data.Metrics;
using Moongate.Server.Interfaces.Services.Metrics;

namespace Moongate.Server.Services.Metrics.Providers;

/// <summary>
/// Exposes timer-wheel runtime metrics.
/// </summary>
public sealed class TimerMetricsProvider : IMetricProvider
{
    private readonly ITimerMetricsSource _timerMetricsSource;

    public TimerMetricsProvider(ITimerMetricsSource timerMetricsSource)
        => _timerMetricsSource = timerMetricsSource;

    public string ProviderName => "timer";

    public ValueTask<IReadOnlyList<MetricSample>> CollectAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = _timerMetricsSource.GetMetricsSnapshot();

        return ValueTask.FromResult<IReadOnlyList<MetricSample>>(
            [
                new("active.count", snapshot.ActiveTimerCount),
                new("registered.total", snapshot.TotalRegisteredTimers),
                new("callbacks.executed.total", snapshot.TotalExecutedCallbacks),
                new("callbacks.errors.total", snapshot.CallbackErrors),
                new("callback.duration.avg_ms", snapshot.AverageCallbackDurationMs)
            ]
        );
    }
}
