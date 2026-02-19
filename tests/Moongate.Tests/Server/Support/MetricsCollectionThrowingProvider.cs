using Moongate.Server.Data.Metrics;
using Moongate.Server.Interfaces.Services.Metrics;

namespace Moongate.Tests.Server.Support;

public sealed class MetricsCollectionThrowingProvider : IMetricProvider
{
    public MetricsCollectionThrowingProvider(string providerName)
    {
        ProviderName = providerName;
    }

    public string ProviderName { get; }

    public ValueTask<IReadOnlyList<MetricSample>> CollectAsync(CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Synthetic test failure");
}
