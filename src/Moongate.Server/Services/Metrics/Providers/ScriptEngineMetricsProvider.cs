using Moongate.Scripting.Interfaces;
using Moongate.Server.Data.Metrics;
using Moongate.Server.Interfaces.Services.Metrics;

namespace Moongate.Server.Services.Metrics.Providers;

/// <summary>
/// Exposes scripting engine cache and execution metrics.
/// </summary>
public sealed class ScriptEngineMetricsProvider : IMetricProvider
{
    private readonly IScriptEngineService _scriptEngineService;

    public ScriptEngineMetricsProvider(IScriptEngineService scriptEngineService)
        => _scriptEngineService = scriptEngineService;

    public string ProviderName => "scripting";

    public ValueTask<IReadOnlyList<MetricSample>> CollectAsync(CancellationToken cancellationToken = default)
    {
        var metrics = _scriptEngineService.GetExecutionMetrics();

        return ValueTask.FromResult<IReadOnlyList<MetricSample>>(
            [
                new("execution.time_ms", metrics.ExecutionTimeMs),
                new("memory.used_bytes", metrics.MemoryUsedBytes),
                new("statements.executed", metrics.StatementsExecuted),
                new("cache.hits.total", metrics.CacheHits),
                new("cache.misses.total", metrics.CacheMisses),
                new("cache.entries.total", metrics.TotalScriptsCached)
            ]
        );
    }
}
