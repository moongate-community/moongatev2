using MemoryPack;
using Moongate.Persistence.Data.Persistence;
using Moongate.Persistence.Interfaces.Persistence;
using Serilog;

namespace Moongate.Persistence.Services.Persistence;

/// <summary>
/// Persists full world snapshots using MemoryPack binary serialization.
/// </summary>
public sealed class MemoryPackSnapshotService : ISnapshotService
{
    private readonly ILogger _logger = Log.ForContext<MemoryPackSnapshotService>();
    private readonly string _snapshotFilePath;

    public MemoryPackSnapshotService(string snapshotFilePath)
        => _snapshotFilePath = snapshotFilePath;

    public async ValueTask<WorldSnapshot?> LoadAsync(CancellationToken cancellationToken = default)
    {
        _logger.Verbose("Snapshot load requested Path={SnapshotPath}", _snapshotFilePath);
        if (!File.Exists(_snapshotFilePath))
        {
            _logger.Verbose("Snapshot file not found Path={SnapshotPath}", _snapshotFilePath);
            return null;
        }

        await using var stream = new FileStream(_snapshotFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var snapshot = await MemoryPackSerializer.DeserializeAsync<WorldSnapshot>(stream, cancellationToken: cancellationToken);
        _logger.Verbose("Snapshot load completed Path={SnapshotPath} Found={Found}", _snapshotFilePath, snapshot is not null);
        return snapshot;
    }

    public async ValueTask SaveAsync(WorldSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        _logger.Verbose(
            "Snapshot save requested Path={SnapshotPath} Accounts={AccountCount} Mobiles={MobileCount} Items={ItemCount}",
            _snapshotFilePath,
            snapshot.Accounts.Length,
            snapshot.Mobiles.Length,
            snapshot.Items.Length
        );
        var directoryPath = Path.GetDirectoryName(_snapshotFilePath);

        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var tempPath = _snapshotFilePath + ".tmp";

        await using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await MemoryPackSerializer.SerializeAsync(stream, snapshot, cancellationToken: cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }

        File.Move(tempPath, _snapshotFilePath, true);
        _logger.Verbose("Snapshot save completed Path={SnapshotPath}", _snapshotFilePath);
    }
}
