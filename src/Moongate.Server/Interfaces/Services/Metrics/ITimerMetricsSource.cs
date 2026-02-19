using Moongate.Server.Data.Metrics;

namespace Moongate.Server.Interfaces.Services.Metrics;

/// <summary>
/// Provides timer-wheel metrics snapshots.
/// </summary>
public interface ITimerMetricsSource
{
    /// <summary>
    /// Gets the latest timer-wheel metrics snapshot.
    /// </summary>
    TimerMetricsSnapshot GetMetricsSnapshot();
}
