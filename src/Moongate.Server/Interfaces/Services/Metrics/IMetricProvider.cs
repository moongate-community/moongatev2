using Moongate.Server.Metrics.Data;

namespace Moongate.Server.Interfaces.Services.Metrics;

/// <summary>
/// Provides metric samples for one subsystem domain.
/// </summary>
public interface IMetricProvider
{
    /// <summary>
    /// Gets the unique provider name used for metric name prefixes.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Collects current metric samples.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Metric samples for this provider.</returns>
    ValueTask<IReadOnlyList<MetricSample>> CollectAsync(CancellationToken cancellationToken = default);
}
