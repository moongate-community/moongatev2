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
        return ValueTask.FromResult(snapshot.ToMetricSamples());
    }
}
