using MemoryPack;
using Moongate.Persistence.Data.Persistence;
using Moongate.Persistence.Interfaces.Persistence;

namespace Moongate.Persistence.Services.Persistence;

/// <summary>
/// Persists full world snapshots using MemoryPack binary serialization.
/// </summary>
public sealed class MemoryPackSnapshotService : ISnapshotService
{
    private readonly string _snapshotFilePath;

    public MemoryPackSnapshotService(string snapshotFilePath)
        => _snapshotFilePath = snapshotFilePath;

    public async ValueTask<WorldSnapshot?> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_snapshotFilePath))
        {
            return null;
        }

        await using var stream = new FileStream(_snapshotFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        return await MemoryPackSerializer.DeserializeAsync<WorldSnapshot>(stream, cancellationToken: cancellationToken);
    }

    public async ValueTask SaveAsync(WorldSnapshot snapshot, CancellationToken cancellationToken = default)
    {
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
    }
}
