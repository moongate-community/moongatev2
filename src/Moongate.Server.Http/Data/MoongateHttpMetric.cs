using Moongate.Server.Metrics.Data;

namespace Moongate.Server.Http.Data;

/// <summary>
/// Represents one flattened metric entry exposed by the HTTP service.
/// </summary>
public sealed class MoongateHttpMetric
{
    public required string Name { get; init; }

    public required double Value { get; init; }

    public DateTimeOffset? Timestamp { get; init; }

    public IReadOnlyDictionary<string, string>? Tags { get; init; }

    public MetricType Type { get; init; } = MetricType.Gauge;

    public string? Help { get; init; }
}
