using System.Buffers.Binary;
using MemoryPack;
using Moongate.Persistence.Data.Persistence;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Persistence.Utils;

namespace Moongate.Persistence.Services.Persistence;

/// <summary>
/// Stores append-only journal entries in a binary file with checksum validation.
/// </summary>
public sealed class BinaryJournalService : IJournalService, IDisposable
{
    private readonly string _journalFilePath;
    private readonly SemaphoreSlim _ioLock = new(1, 1);

    public BinaryJournalService(string journalFilePath)
        => _journalFilePath = journalFilePath;

    public async ValueTask AppendAsync(JournalEntry entry, CancellationToken cancellationToken = default)
    {
        var payload = MemoryPackSerializer.Serialize(entry);
        var checksum = ChecksumUtils.Compute(payload);

        var lengthBuffer = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(lengthBuffer, payload.Length);

        var checksumBuffer = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(checksumBuffer, checksum);

        var directoryPath = Path.GetDirectoryName(_journalFilePath);

        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        await _ioLock.WaitAsync(cancellationToken);

        try
        {
            await using var stream = new FileStream(_journalFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            await stream.WriteAsync(lengthBuffer, cancellationToken);
            await stream.WriteAsync(payload, cancellationToken);
            await stream.WriteAsync(checksumBuffer, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    public void Dispose()
    {
        _ioLock.Dispose();
    }

    public async ValueTask<IReadOnlyCollection<JournalEntry>> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_journalFilePath))
        {
            return [];
        }

        var entries = new List<JournalEntry>();

        await _ioLock.WaitAsync(cancellationToken);

        try
        {
            await using var stream = new FileStream(_journalFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var lengthBuffer = new byte[4];
            var checksumBuffer = new byte[4];

            while (true)
            {
                var lengthBytesRead = await stream.ReadAsync(lengthBuffer, cancellationToken);

                if (lengthBytesRead == 0)
                {
                    break;
                }

                if (lengthBytesRead != 4)
                {
                    break;
                }

                var payloadLength = BinaryPrimitives.ReadInt32LittleEndian(lengthBuffer);

                if (payloadLength <= 0 || payloadLength > 16 * 1024 * 1024)
                {
                    break;
                }

                var payload = new byte[payloadLength];
                var payloadBytesRead = await stream.ReadAsync(payload, cancellationToken);

                if (payloadBytesRead != payloadLength)
                {
                    break;
                }

                var checksumBytesRead = await stream.ReadAsync(checksumBuffer, cancellationToken);

                if (checksumBytesRead != 4)
                {
                    break;
                }

                var expectedChecksum = BinaryPrimitives.ReadUInt32LittleEndian(checksumBuffer);
                var actualChecksum = ChecksumUtils.Compute(payload);

                if (expectedChecksum != actualChecksum)
                {
                    break;
                }

                var entry = MemoryPackSerializer.Deserialize<JournalEntry>(payload);

                if (entry is null)
                {
                    break;
                }

                entries.Add(entry);
            }
        }
        finally
        {
            _ioLock.Release();
        }

        return entries;
    }

    public async ValueTask ResetAsync(CancellationToken cancellationToken = default)
    {
        await _ioLock.WaitAsync(cancellationToken);

        try
        {
            var directoryPath = Path.GetDirectoryName(_journalFilePath);

            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            await using var stream = new FileStream(_journalFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            await stream.FlushAsync(cancellationToken);
        }
        finally
        {
            _ioLock.Release();
        }
    }
}
