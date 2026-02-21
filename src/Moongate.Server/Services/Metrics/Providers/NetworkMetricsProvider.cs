using Moongate.Server.Data.Metrics;
using Moongate.Server.Interfaces.Services.Metrics;

namespace Moongate.Server.Services.Metrics.Providers;

/// <summary>
/// Exposes network and parser metrics.
/// </summary>
public sealed class NetworkMetricsProvider : IMetricProvider
{
    private readonly INetworkMetricsSource _networkMetricsSource;

    public NetworkMetricsProvider(INetworkMetricsSource networkMetricsSource)
        => _networkMetricsSource = networkMetricsSource;

    public string ProviderName => "network";

    public ValueTask<IReadOnlyList<MetricSample>> CollectAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = _networkMetricsSource.GetMetricsSnapshot();
        return ValueTask.FromResult(snapshot.ToMetricSamples());
    }
}
