using Moongate.Server.Data.Metrics.Attributes;
using Moongate.Server.Data.Metrics.Types;

namespace Moongate.Server.Data.Metrics;

/// <summary>
/// Immutable snapshot of persistence snapshot-save metrics.
/// </summary>
public sealed class PersistenceMetricsSnapshot
{
    public PersistenceMetricsSnapshot(
        long totalSaves,
        double lastSaveDurationMs,
        DateTimeOffset? lastSaveTimestampUtc,
        long saveErrors
    )
    {
        TotalSaves = totalSaves;
        LastSaveDurationMs = lastSaveDurationMs;
        LastSaveTimestampUtc = lastSaveTimestampUtc;
        SaveErrors = saveErrors;
    }

    [Metric("snapshot.saves.total")]
    public long TotalSaves { get; }

    [Metric("persistence.save.duration.last_ms", Aliases = new[] { "snapshot.save.duration.last_ms" })]
    public double LastSaveDurationMs { get; }

    [Metric("snapshot.save.timestamp_utc_ms", Transform = MetricValueTransformType.UnixTimeMillisecondsOrZero)]
    public DateTimeOffset? LastSaveTimestampUtc { get; }

    [Metric("snapshot.save.errors.total")]
    public long SaveErrors { get; }
}
