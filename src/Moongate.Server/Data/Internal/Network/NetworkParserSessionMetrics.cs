namespace Moongate.Server.Data.Internal.Network;

internal sealed class NetworkParserSessionMetrics
{
    private long _receivedBytes;
    private int _parsedPackets;
    private int _unknownOpcodeDrops;
    private int _invalidLengthDrops;
    private int _parseFailures;
    private int _protocolViolations;
    private int _pendingBufferOverflows;

    public long ReceivedBytes => Volatile.Read(ref _receivedBytes);
    public int ParsedPackets => Volatile.Read(ref _parsedPackets);
    public int UnknownOpcodeDrops => Volatile.Read(ref _unknownOpcodeDrops);
    public int InvalidLengthDrops => Volatile.Read(ref _invalidLengthDrops);
    public int ParseFailures => Volatile.Read(ref _parseFailures);
    public int ProtocolViolations => Volatile.Read(ref _protocolViolations);
    public int PendingBufferOverflows => Volatile.Read(ref _pendingBufferOverflows);

    public void AddReceivedBytes(int bytes)
        => Interlocked.Add(ref _receivedBytes, bytes);

    public void IncrementParsedPackets()
        => Interlocked.Increment(ref _parsedPackets);

    public void IncrementUnknownOpcodeDrops()
        => Interlocked.Increment(ref _unknownOpcodeDrops);

    public void IncrementInvalidLengthDrops()
        => Interlocked.Increment(ref _invalidLengthDrops);

    public void IncrementParseFailures()
        => Interlocked.Increment(ref _parseFailures);

    public int IncrementProtocolViolations()
        => Interlocked.Increment(ref _protocolViolations);

    public void IncrementPendingBufferOverflows()
        => Interlocked.Increment(ref _pendingBufferOverflows);
}
