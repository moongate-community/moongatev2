using System.Buffers.Binary;
using System.Collections.Concurrent;
using MemoryPack;
using Moongate.Persistence.Data.Persistence;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Persistence.Utils;
using Serilog;

namespace Moongate.Persistence.Services.Persistence;

/// <summary>
/// Stores append-only journal entries in a binary file with checksum validation.
/// </summary>
public sealed class BinaryJournalService : IJournalService
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> IoLocks = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger _logger = Log.ForContext<BinaryJournalService>();
    private readonly string _journalFilePath;
    private readonly SemaphoreSlim _ioLock;

    public BinaryJournalService(string journalFilePath)
    {
        _journalFilePath = Path.GetFullPath(journalFilePath);
        _ioLock = IoLocks.GetOrAdd(_journalFilePath, _ => new SemaphoreSlim(1, 1));
    }

    public async ValueTask AppendAsync(JournalEntry entry, CancellationToken cancellationToken = default)
    {
        _logger.Verbose(
            "Journal append requested Path={JournalPath} SequenceId={SequenceId} OperationType={OperationType}",
            _journalFilePath,
            entry.SequenceId,
            entry.OperationType
        );
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

        _logger.Verbose("Journal append completed Path={JournalPath} SequenceId={SequenceId}", _journalFilePath, entry.SequenceId);
    }

    public async ValueTask<IReadOnlyCollection<JournalEntry>> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.Verbose("Journal read-all requested Path={JournalPath}", _journalFilePath);
        if (!File.Exists(_journalFilePath))
        {
            _logger.Verbose("Journal file not found Path={JournalPath}", _journalFilePath);
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
                    _logger.Warning("Journal truncated at record-length read Path={JournalPath}", _journalFilePath);
                    break;
                }

                var payloadLength = BinaryPrimitives.ReadInt32LittleEndian(lengthBuffer);

                if (payloadLength <= 0 || payloadLength > 16 * 1024 * 1024)
                {
                    _logger.Warning("Journal invalid payload length Path={JournalPath} PayloadLength={PayloadLength}", _journalFilePath, payloadLength);
                    break;
                }

                var payload = new byte[payloadLength];
                var payloadBytesRead = await stream.ReadAsync(payload, cancellationToken);

                if (payloadBytesRead != payloadLength)
                {
                    _logger.Warning("Journal truncated at payload read Path={JournalPath} PayloadLength={PayloadLength}", _journalFilePath, payloadLength);
                    break;
                }

                var checksumBytesRead = await stream.ReadAsync(checksumBuffer, cancellationToken);

                if (checksumBytesRead != 4)
                {
                    _logger.Warning("Journal truncated at checksum read Path={JournalPath}", _journalFilePath);
                    break;
                }

                var expectedChecksum = BinaryPrimitives.ReadUInt32LittleEndian(checksumBuffer);
                var actualChecksum = ChecksumUtils.Compute(payload);

                if (expectedChecksum != actualChecksum)
                {
                    _logger.Warning("Journal checksum mismatch Path={JournalPath}", _journalFilePath);
                    break;
                }

                var entry = MemoryPackSerializer.Deserialize<JournalEntry>(payload);

                if (entry is null)
                {
                    _logger.Warning("Journal entry deserialize failed Path={JournalPath}", _journalFilePath);
                    break;
                }

                entries.Add(entry);
            }
        }
        finally
        {
            _ioLock.Release();
        }

        _logger.Verbose("Journal read-all completed Path={JournalPath} Count={Count}", _journalFilePath, entries.Count);
        return entries;
    }

    public async ValueTask ResetAsync(CancellationToken cancellationToken = default)
    {
        _logger.Verbose("Journal reset requested Path={JournalPath}", _journalFilePath);
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

        _logger.Verbose("Journal reset completed Path={JournalPath}", _journalFilePath);
    }
}
