using Moongate.Core.Extensions.Logger;
using Moongate.Server.Data.Config;
using Moongate.Server.Metrics.Data;
using Moongate.Server.Interfaces.Services.Metrics;
using Serilog;

namespace Moongate.Server.Services.Metrics;

/// <summary>
/// Periodically collects metrics from all registered providers and stores the latest snapshot.
/// </summary>
public sealed class MetricsCollectionService : IMetricsCollectionService, IDisposable
{
    private readonly ILogger _logger = Log.ForContext<MetricsCollectionService>();
    private readonly ILogger _metricsLogger = Log.ForContext<MetricsCollectionService>().ForContext("MetricsData", true);
    private readonly IReadOnlyList<IMetricProvider> _providers;
    private readonly MoongateMetricsConfig _config;

    private readonly Lock _sync = new();
    private CancellationTokenSource _lifetimeCts = new();
    private Task _collectionTask = Task.CompletedTask;

    private MetricsSnapshot _snapshot = new(
        DateTimeOffset.MinValue,
        new Dictionary<string, MetricSample>(StringComparer.Ordinal)
    );

    public MetricsCollectionService(IEnumerable<IMetricProvider> providers, MoongateMetricsConfig config)
    {
        _providers = [.. providers];
        _config = config;
    }

    public void Dispose()
    {
        _lifetimeCts.Dispose();
        GC.SuppressFinalize(this);
    }

    public IReadOnlyDictionary<string, MetricSample> GetAllMetrics()
    {
        lock (_sync)
        {
            return _snapshot.Metrics;
        }
    }

    public MetricsSnapshot GetSnapshot()
    {
        lock (_sync)
        {
            return _snapshot;
        }
    }

    public Task StartAsync()
    {
        if (!_config.Enabled)
        {
            return Task.CompletedTask;
        }

        if (_lifetimeCts.IsCancellationRequested)
        {
            _lifetimeCts.Dispose();
            _lifetimeCts = new();
        }

        _collectionTask = Task.Run(() => RunCollectionLoopAsync(_lifetimeCts.Token), _lifetimeCts.Token);

        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        await _lifetimeCts.CancelAsync();

        try
        {
            await _collectionTask;
        }
        catch (OperationCanceledException)
        {
            // expected during shutdown
        }
    }

    private async Task CollectOnceAsync(CancellationToken cancellationToken)
    {
        var values = new Dictionary<string, MetricSample>(StringComparer.Ordinal);
        var now = DateTimeOffset.UtcNow;

        foreach (var provider in _providers)
        {
            try
            {
                var samples = await provider.CollectAsync(cancellationToken);

                foreach (var sample in samples)
                {
                    var key = CreateMetricKey(provider.ProviderName, sample.Name);
                    values[key] = sample with { Timestamp = sample.Timestamp ?? now };
                }

                if (_config.LogEnabled)
                {
                    _metricsLogger.Write(
                        _config.LogLevel.ToSerilogLogLevel(),
                        "Collected {MetricCount} metrics from provider {ProviderName}",
                        samples.Count,
                        provider.ProviderName
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Metrics provider collection failed for {ProviderName}", provider.ProviderName);
            }
        }

        lock (_sync)
        {
            _snapshot = new(now, values);
        }
    }

    private static string CreateMetricKey(string providerName, string metricName)
        => string.IsNullOrWhiteSpace(providerName) ? metricName :
           string.IsNullOrWhiteSpace(metricName) ? providerName : providerName + "." + metricName;

    private async Task RunCollectionLoopAsync(CancellationToken cancellationToken)
    {
        await CollectOnceAsync(cancellationToken);

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(Math.Max(1, _config.IntervalMilliseconds)));

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            await CollectOnceAsync(cancellationToken);
        }
    }
}
