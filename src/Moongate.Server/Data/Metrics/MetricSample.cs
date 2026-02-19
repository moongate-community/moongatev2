namespace Moongate.Server.Data.Metrics;

/// <summary>
/// Represents one collected metric data point.
/// </summary>
public sealed record MetricSample(
    string Name,
    double Value,
    DateTimeOffset? Timestamp = null,
    IReadOnlyDictionary<string, string>? Tags = null
);
