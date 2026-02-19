namespace Moongate.Server.Http.Data;

/// <summary>
/// HTTP-facing metrics snapshot payload source.
/// </summary>
public sealed class MoongateHttpMetricsSnapshot
{
    public required DateTimeOffset CollectedAt { get; init; }

    public required IReadOnlyDictionary<string, MoongateHttpMetric> Metrics { get; init; }
}
