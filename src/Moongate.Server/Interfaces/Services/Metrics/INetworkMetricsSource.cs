using Moongate.Server.Data.Metrics;

namespace Moongate.Server.Interfaces.Services.Metrics;

/// <summary>
/// Provides network metrics snapshots.
/// </summary>
public interface INetworkMetricsSource
{
    /// <summary>
    /// Gets the latest network metrics snapshot.
    /// </summary>
    NetworkMetricsSnapshot GetMetricsSnapshot();
}
