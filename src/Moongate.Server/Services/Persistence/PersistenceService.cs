using System.Diagnostics;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Persistence.Data.Persistence;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Persistence.Services.Persistence;
using Moongate.Server.Data.Config;
using Moongate.Server.Data.Metrics;
using Moongate.Server.Interfaces.Services.Metrics;
using Moongate.Server.Interfaces.Services.Persistence;
using Moongate.Server.Interfaces.Services.Timing;
using Serilog;

namespace Moongate.Server.Services.Persistence;

/// <summary>
/// Wraps persistence unit-of-work lifecycle for host-managed startup and shutdown.
/// </summary>
public sealed class PersistenceService : IPersistenceService, IPersistenceMetricsSource
{
    private readonly ILogger _logger = Log.ForContext<PersistenceService>();
    private readonly DirectoriesConfig _directoriesConfig;
    private readonly Lock _metricsSync = new();
    private PersistenceMetricsSnapshot _metricsSnapshot = new(0, 0, null, 0);

    private readonly ITimerService _timerService;
    private readonly MoongatePersistenceConfig _persistenceConfig;
    private string? _dbSaveTimerId;

    public PersistenceService(
        DirectoriesConfig directoriesConfig,
        ITimerService timerService,
        MoongateConfig moongateConfig
    )
    {
        ArgumentNullException.ThrowIfNull(directoriesConfig);
        ArgumentNullException.ThrowIfNull(timerService);
        ArgumentNullException.ThrowIfNull(moongateConfig);
        ArgumentNullException.ThrowIfNull(moongateConfig.Persistence);

        _directoriesConfig = directoriesConfig;
        _timerService = timerService;
        _persistenceConfig = moongateConfig.Persistence;

        var saveDirectory = directoriesConfig[DirectoryType.Save];
        var options = new PersistenceOptions(
            Path.Combine(saveDirectory, "world.snapshot.bin"),
            Path.Combine(saveDirectory, "world.journal.bin")
        );

        UnitOfWork = new PersistenceUnitOfWork(options);
    }

    public IPersistenceUnitOfWork UnitOfWork { get; }

    public PersistenceMetricsSnapshot GetMetricsSnapshot()
    {
        lock (_metricsSync)
        {
            return _metricsSnapshot;
        }
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        _logger.Verbose("Persistence service save requested");
        await SaveSnapshotWithMetricsAsync(cancellationToken);
        _logger.Verbose(
            "Persistence service save completed in {ElapsedMs} ms (TotalSaves={TotalSaves}, SaveErrors={SaveErrors})",
            GetMetricsSnapshot().LastSaveDurationMs,
            GetMetricsSnapshot().TotalSaves,
            GetMetricsSnapshot().SaveErrors
        );
    }

    public async Task StartAsync()
    {
        _logger.Verbose("Persistence service start requested");
        await UnitOfWork.InitializeAsync();

        _dbSaveTimerId ??= _timerService.RegisterTimer(
            "db_save",
            TimeSpan.FromSeconds(Math.Max(1, _persistenceConfig.SaveIntervalSeconds)),
            () =>
            {
                try
                {
                    SaveSnapshotWithMetricsAsync().GetAwaiter().GetResult();
                    _logger.Debug("Automatic DB save completed in {ElapsedMs} ms", GetMetricsSnapshot().LastSaveDurationMs);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Automatic DB save timer failed.");
                }
            },
            repeat: true
        );

        _logger.Verbose("Persistence service start completed");
        _logger.Information(
            "Persistence service started in directory: {SaveDirectory}",
            _directoriesConfig[DirectoryType.Save]
        );
    }

    public async Task StopAsync()
    {
        _logger.Verbose("Persistence service stop requested");

        if (!string.IsNullOrWhiteSpace(_dbSaveTimerId))
        {
            _timerService.UnregisterTimer(_dbSaveTimerId);
            _dbSaveTimerId = null;
        }

        await SaveSnapshotWithMetricsAsync();
        _logger.Verbose("Persistence service stop completed");
    }

    private async Task SaveSnapshotWithMetricsAsync(CancellationToken cancellationToken = default)
    {
        var start = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await UnitOfWork.SaveSnapshotAsync(cancellationToken);

            lock (_metricsSync)
            {
                _metricsSnapshot = _metricsSnapshot with
                {
                    TotalSaves = _metricsSnapshot.TotalSaves + 1,
                    LastSaveDurationMs = stopwatch.Elapsed.TotalMilliseconds,
                    LastSaveTimestampUtc = start
                };
            }
        }
        catch
        {
            lock (_metricsSync)
            {
                _metricsSnapshot = _metricsSnapshot with
                {
                    SaveErrors = _metricsSnapshot.SaveErrors + 1
                };
            }

            throw;
        }
    }

    public void Dispose()
    {
        if (UnitOfWork is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
