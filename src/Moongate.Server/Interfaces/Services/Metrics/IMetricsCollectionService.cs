using Moongate.Abstractions.Interfaces.Services.Base;
using Moongate.Server.Metrics.Data;

namespace Moongate.Server.Interfaces.Services.Metrics;

/// <summary>
/// Collects and exposes the latest server metrics snapshot.
/// </summary>
public interface IMetricsCollectionService : IMoongateService
{
    /// <summary>
    /// Returns flattened metric entries by name.
    /// </summary>
    /// <returns>Dictionary keyed by metric name.</returns>
    IReadOnlyDictionary<string, MetricSample> GetAllMetrics();

    /// <summary>
    /// Returns the last collected snapshot.
    /// </summary>
    /// <returns>Current metrics snapshot.</returns>
    MetricsSnapshot GetSnapshot();
}
