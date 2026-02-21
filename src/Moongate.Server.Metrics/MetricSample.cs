namespace Moongate.Server.Metrics.Data;

/// <summary>
/// Represents one collected metric data point.
/// </summary>
public sealed record MetricSample(
    string Name,
    double Value,
    DateTimeOffset? Timestamp = null,
    IReadOnlyDictionary<string, string>? Tags = null,
    MetricType Type = MetricType.Gauge,
    string? Help = null
);
