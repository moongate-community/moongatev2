using Moongate.Server.Metrics.Data;
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
        return ValueTask.FromResult(snapshot.ToMetricSamples());
    }
}
