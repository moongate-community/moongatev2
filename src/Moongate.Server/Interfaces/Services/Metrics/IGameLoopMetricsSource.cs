using Moongate.Server.Data.Metrics;

namespace Moongate.Server.Interfaces.Services.Metrics;

/// <summary>
/// Provides game-loop metrics snapshots.
/// </summary>
public interface IGameLoopMetricsSource
{
    /// <summary>
    /// Gets the latest game-loop metrics snapshot.
    /// </summary>
    GameLoopMetricsSnapshot GetMetricsSnapshot();
}
