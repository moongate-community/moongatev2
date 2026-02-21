using Moongate.Server.Metrics.Data;

namespace Moongate.Server.Interfaces.Services.Metrics;

/// <summary>
/// Provides persistence metrics snapshots.
/// </summary>
public interface IPersistenceMetricsSource
{
    /// <summary>
    /// Gets the latest persistence metrics snapshot.
    /// </summary>
    PersistenceMetricsSnapshot GetMetricsSnapshot();
}
