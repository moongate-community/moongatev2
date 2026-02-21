using Moongate.Server.Data.Metrics;
using Moongate.Server.Interfaces.Services.Metrics;

namespace Moongate.Server.Services.Metrics.Providers;

/// <summary>
/// Exposes game-loop runtime metrics.
/// </summary>
public sealed class GameLoopMetricsProvider : IMetricProvider
{
    private readonly IGameLoopMetricsSource _gameLoopMetricsSource;

    public GameLoopMetricsProvider(IGameLoopMetricsSource gameLoopMetricsSource)
        => _gameLoopMetricsSource = gameLoopMetricsSource;

    public string ProviderName => "gameloop";

    public ValueTask<IReadOnlyList<MetricSample>> CollectAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = _gameLoopMetricsSource.GetMetricsSnapshot();

        return ValueTask.FromResult<IReadOnlyList<MetricSample>>(
            [
                new("loop.tick.duration.avg_ms", snapshot.AverageTickMs),
                new("loop.tick.duration.max_ms", snapshot.MaxTickMs),
                new("loop.idle.sleep.count", snapshot.IdleSleepCount),
                new("loop.work.units.avg", snapshot.AverageWorkUnits),
                new("network.outbound.queue.depth", snapshot.OutboundQueueDepth),
                new("network.outbound.packets.total", snapshot.OutboundPacketsTotal),
                new("ticks.total", snapshot.TickCount),
                new("tick.duration.avg_ms", snapshot.AverageTickMs),
                new("uptime.ms", snapshot.Uptime.TotalMilliseconds)
            ]
        );
    }
}
