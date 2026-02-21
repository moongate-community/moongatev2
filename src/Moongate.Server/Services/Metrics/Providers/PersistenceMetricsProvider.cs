using Moongate.Server.Data.Metrics;
using Moongate.Server.Interfaces.Services.Metrics;

namespace Moongate.Server.Services.Metrics.Providers;

/// <summary>
/// Exposes persistence snapshot save metrics.
/// </summary>
public sealed class PersistenceMetricsProvider : IMetricProvider
{
    private readonly IPersistenceMetricsSource _persistenceMetricsSource;

    public PersistenceMetricsProvider(IPersistenceMetricsSource persistenceMetricsSource)
        => _persistenceMetricsSource = persistenceMetricsSource;

    public string ProviderName => "persistence";

    public ValueTask<IReadOnlyList<MetricSample>> CollectAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = _persistenceMetricsSource.GetMetricsSnapshot();
        var timestampUtcMs = snapshot.LastSaveTimestampUtc?.ToUnixTimeMilliseconds() ?? 0;

        return ValueTask.FromResult<IReadOnlyList<MetricSample>>(
            [
                new("persistence.save.duration.last_ms", snapshot.LastSaveDurationMs),
                new("snapshot.saves.total", snapshot.TotalSaves),
                new("snapshot.save.duration.last_ms", snapshot.LastSaveDurationMs),
                new("snapshot.save.timestamp_utc_ms", timestampUtcMs),
                new("snapshot.save.errors.total", snapshot.SaveErrors)
            ]
        );
    }
}
