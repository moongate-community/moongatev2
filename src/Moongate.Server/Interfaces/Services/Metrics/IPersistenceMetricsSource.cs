using Moongate.Server.Data.Metrics;

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
