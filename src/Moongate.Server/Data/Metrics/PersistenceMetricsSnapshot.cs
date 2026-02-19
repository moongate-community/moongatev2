namespace Moongate.Server.Data.Metrics;

/// <summary>
/// Immutable snapshot of persistence snapshot-save metrics.
/// </summary>
public readonly record struct PersistenceMetricsSnapshot(
    long TotalSaves,
    double LastSaveDurationMs,
    DateTimeOffset? LastSaveTimestampUtc,
    long SaveErrors
);
