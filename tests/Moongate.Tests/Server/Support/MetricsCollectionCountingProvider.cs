using Moongate.Server.Data.Metrics;
using Moongate.Server.Interfaces.Services.Metrics;

namespace Moongate.Tests.Server.Support;

public sealed class MetricsCollectionCountingProvider : IMetricProvider
{
    private readonly string _metricName;
    private readonly double _value;

    public MetricsCollectionCountingProvider(string providerName, string metricName, double value)
    {
        ProviderName = providerName;
        _metricName = metricName;
        _value = value;
    }

    public string ProviderName { get; }

    public ValueTask<IReadOnlyList<MetricSample>> CollectAsync(CancellationToken cancellationToken = default)
        => ValueTask.FromResult<IReadOnlyList<MetricSample>>([new(_metricName, _value)]);
}
