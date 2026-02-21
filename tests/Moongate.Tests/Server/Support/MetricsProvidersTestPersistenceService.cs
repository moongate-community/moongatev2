using Moongate.Server.Metrics.Data;
using Moongate.Server.Interfaces.Services.Metrics;

namespace Moongate.Tests.Server.Support;

public sealed class MetricsProvidersTestPersistenceService : IPersistenceMetricsSource
{
    public long TotalSaves { get; set; }

    public double LastSaveDurationMs { get; set; }

    public DateTimeOffset? LastSaveTimestampUtc { get; set; }

    public long SaveErrors { get; set; }

    public PersistenceMetricsSnapshot GetMetricsSnapshot()
        => new(TotalSaves, LastSaveDurationMs, LastSaveTimestampUtc, SaveErrors);
}
