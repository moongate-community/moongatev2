using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Persistence.Data.Persistence;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Persistence.Services.Persistence;
using Moongate.Server.Data.Metrics;
using Moongate.Server.Interfaces.Services.Metrics;
using Moongate.Server.Interfaces.Services.Persistence;
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

    public PersistenceService(DirectoriesConfig directoriesConfig)
    {
        _directoriesConfig = directoriesConfig;
        ArgumentNullException.ThrowIfNull(directoriesConfig);

        var saveDirectory = directoriesConfig[DirectoryType.Save];
        var options = new PersistenceOptions(
            Path.Combine(saveDirectory, "world.snapshot.bin"),
            Path.Combine(saveDirectory, "world.journal.bin")
        );

        UnitOfWork = new PersistenceUnitOfWork(options);
    }

    public IPersistenceUnitOfWork UnitOfWork { get; }

    public void Dispose()
    {
        if (UnitOfWork is IDisposable disposable)
        {
            disposable.Dispose();
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
        _logger.Verbose("Persistence service start completed");
        _logger.Information(
            "Persistence service started in directory: {SaveDirectory}",
            _directoriesConfig[DirectoryType.Save]
        );
    }

    public async Task StopAsync()
    {
        _logger.Verbose("Persistence service stop requested");
        await SaveSnapshotWithMetricsAsync();
        _logger.Verbose("Persistence service stop completed");
    }

    private async Task SaveSnapshotWithMetricsAsync(CancellationToken cancellationToken = default)
    {
        var start = DateTimeOffset.UtcNow;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

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

    public PersistenceMetricsSnapshot GetMetricsSnapshot()
    {
        lock (_metricsSync)
        {
            return _metricsSnapshot;
        }
    }
}
