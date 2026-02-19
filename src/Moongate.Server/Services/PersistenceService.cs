using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Persistence.Data.Persistence;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Persistence.Services.Persistence;
using Moongate.Server.Interfaces.Services;
using Serilog;

namespace Moongate.Server.Services;

/// <summary>
/// Wraps persistence unit-of-work lifecycle for host-managed startup and shutdown.
/// </summary>
public sealed class PersistenceService : IPersistenceService
{
    private readonly ILogger _logger = Log.ForContext<PersistenceService>();

    public PersistenceService(DirectoriesConfig directoriesConfig)
    {
        ArgumentNullException.ThrowIfNull(directoriesConfig);

        var saveDirectory = directoriesConfig[DirectoryType.Save];
        var options = new PersistenceOptions(
            Path.Combine(saveDirectory, "world.snapshot.bin"),
            Path.Combine(saveDirectory, "world.journal.bin")
        );

        UnitOfWork = new PersistenceUnitOfWork(options);
    }

    public IPersistenceUnitOfWork UnitOfWork { get; }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        _logger.Verbose("Persistence service save requested");
        await UnitOfWork.SaveSnapshotAsync(cancellationToken);
        _logger.Verbose("Persistence service save completed");
    }

    public async Task StartAsync()
    {
        _logger.Verbose("Persistence service start requested");
        await UnitOfWork.InitializeAsync();
        _logger.Verbose("Persistence service start completed");
    }

    public async Task StopAsync()
    {
        _logger.Verbose("Persistence service stop requested");
        await UnitOfWork.SaveSnapshotAsync();
        _logger.Verbose("Persistence service stop completed");
    }

    public void Dispose()
    {
        if (UnitOfWork is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
