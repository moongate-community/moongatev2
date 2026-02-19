namespace Moongate.Server.Data.Metrics;

/// <summary>
/// Holds the last metrics snapshot collected by the metrics service.
/// </summary>
public sealed class MetricsSnapshot
{
    public MetricsSnapshot(DateTimeOffset collectedAt, IReadOnlyDictionary<string, MetricSample> metrics)
    {
        CollectedAt = collectedAt;
        Metrics = metrics;
    }

    public DateTimeOffset CollectedAt { get; }

    public IReadOnlyDictionary<string, MetricSample> Metrics { get; }
}
