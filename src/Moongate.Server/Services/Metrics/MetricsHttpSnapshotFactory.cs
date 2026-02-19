using Moongate.Server.Http.Data;
using Moongate.Server.Interfaces.Services.Metrics;

namespace Moongate.Server.Services.Metrics;

/// <summary>
/// Default mapper from internal metrics snapshots to HTTP payload snapshots.
/// </summary>
public sealed class MetricsHttpSnapshotFactory : IMetricsHttpSnapshotFactory
{
    private readonly IMetricsCollectionService _metricsCollectionService;

    public MetricsHttpSnapshotFactory(IMetricsCollectionService metricsCollectionService)
    {
        _metricsCollectionService = metricsCollectionService;
    }

    public MoongateHttpMetricsSnapshot? CreateSnapshot()
    {
        var snapshot = _metricsCollectionService.GetSnapshot();
        var metrics = snapshot.Metrics.ToDictionary(
            static pair => pair.Key,
            static pair =>
                new MoongateHttpMetric
                {
                    Name = pair.Value.Name,
                    Value = pair.Value.Value,
                    Timestamp = pair.Value.Timestamp,
                    Tags = pair.Value.Tags
                },
            StringComparer.Ordinal
        );

        return new MoongateHttpMetricsSnapshot
        {
            CollectedAt = snapshot.CollectedAt,
            Metrics = metrics
        };
    }
}
