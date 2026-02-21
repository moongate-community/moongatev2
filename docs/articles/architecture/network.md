# Network System

Deep dive into Moongate v2's networking architecture.

## Overview

Moongate v2 implements a custom TCP server optimized for the Ultima Online protocol. The network layer handles:

- Client connections and session management
- Packet framing and parsing
- Zero-allocation I/O using `Span<T>`
- Thread-safe message passing to the game loop

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      Network Layer                           │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌────────────┐    ┌────────────┐    ┌────────────┐         │
│  │  GameTcp   │───▶│  GameNet   │───▶│  Packet    │         │
│  │  Server    │    │  Session   │    │  Handlers  │         │
│  └────────────┘    └────────────┘    └────────────┘         │
│                           │                    │             │
│                           │                    ▼             │
│                      ┌────┴────┐    ┌────────────┐          │
│                      │ Buffer  │    │  Message   │          │
│                      │  Pool   │    │   Bus      │          │
│                      └─────────┘    └────────────┘          │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## Components

### GameTcpServer

The main TCP server responsible for:

- Listening on configured port (default: 2593)
- Accepting incoming client connections
- Creating `GameNetworkSession` instances
- Managing connection lifecycle

```csharp
public sealed class GameTcpServer
{
    private readonly TcpListener _listener;
    private readonly ConcurrentDictionary<Serial, GameNetworkSession> _sessions;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _listener.Start();
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var tcpClient = await _listener.AcceptTcpClientAsync(cancellationToken);
            var session = new GameNetworkSession(tcpClient, ...);
            _ = session.RunAsync(cancellationToken);
        }
    }
}
```

### GameNetworkSession

Per-client network session handling:

- Socket I/O (receive/send)
- Packet framing (length-prefixed)
- Packet parsing via `SpanReader`
- Session state management

```csharp
public sealed class GameNetworkSession
{
    private readonly Socket _socket;
    private readonly byte[] _receiveBuffer;
    private GameSession? _gameSession;  // Linked gameplay session
    
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var received = await _socket.ReceiveAsync(_receiveBuffer, cancellationToken);
            if (received == 0) break;  // Client disconnected
            
            ProcessReceivedData(_receiveBuffer.AsSpan(0, received));
        }
    }
    
    private void ProcessReceivedData(ReadOnlySpan<byte> data)
    {
        var reader = new SpanReader(data);
        var packetId = reader.ReadByte();
        
        // Route to appropriate handler
        _packetHandlerRegistry.Handle(packetId, reader, this);
    }
}
```

### SpanReader / SpanWriter

Zero-allocation binary I/O:

```csharp
public ref struct SpanReader
{
    private ReadOnlySpan<byte> _span;
    
    public byte ReadByte() => _span[0];
    public ushort ReadUInt16() => BinaryPrimitives.ReadUInt16LittleEndian(_span);
    public uint ReadUInt32() => BinaryPrimitives.ReadUInt32LittleEndian(_span);
    public string ReadAscii(int length) => Encoding.ASCII.GetString(_span.Slice(0, length));
}

public ref struct SpanWriter
{
    private Span<byte> _span;
    
    public void Write(byte value) => _span[0] = value;
    public void Write(ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(_span, value);
    public void Write(uint value) => BinaryPrimitives.WriteUInt32LittleEndian(_span, value);
}
```

**Benefits:**
- No heap allocations
- Stack-only structs
- High performance for hot paths

## Packet Handling

### Packet Registration

Packets are registered via attributes and source generators:

```csharp
[PacketHandler(0x02, "Move Request", Length = 7)]
public sealed class MoveRequestHandler : IPacketHandler
{
    public void Handle(SpanReader reader, GameNetworkSession session)
    {
        var sequence = reader.ReadByte();
        var direction = reader.ReadByte();
        // ... process movement
    }
}
```

The source generator creates the packet registry at compile time:

```csharp
// Generated code
public static class PacketRegistry
{
    public static readonly IPacketHandler[] Handlers = new IPacketHandler[256]
    {
        [0x02] = new MoveRequestHandler(),
        // ...
    };
}
```

### Packet Types

#### Fixed-Length Packets

```csharp
[PacketHandler(0x06, "Double Click", Length = 5)]
public sealed class DoubleClickHandler : IPacketHandler
{
    // Always exactly 5 bytes
}
```

#### Variable-Length Packets

```csharp
[PacketHandler(0xAD, "Speech Request", Length = -1)]
public sealed class SpeechRequestHandler : IPacketHandler
{
    // Length specified in packet header
}
```

### Inbound vs Outbound

**Inbound Packets** (Client → Server):
- Handled by `IPacketListener`
- Processed in network thread
- Enqueued to message bus for game loop

**Outbound Packets** (Server → Client):
- Created by event listeners
- Enqueued to `IOutgoingPacketQueue`
- Sent via `GameNetworkSession`

## Message Bus

Bridges network thread and game loop:

```csharp
public interface IMessageBusService
{
    void Enqueue(ReadOnlySpan<byte> packet, GameNetworkSession session);
    bool TryDequeue(out NetworkMessage message);
}

// Network thread
messageBus.Enqueue(packetSpan, session);

// Game loop
while (messageBus.TryDequeue(out var message))
{
    ProcessNetworkMessage(message);
}
```

**Benefits:**
- Thread-safe communication
- No locks in hot path (concurrent queue)
- Deterministic processing order

## Buffer Management

### NetworkBufferPool

Recycles buffers to reduce GC pressure:

```csharp
public sealed class NetworkBufferPool
{
    private readonly ConcurrentBag<byte[]> _pool;
    private readonly int _bufferSize;
    
    public byte[] Rent()
    {
        return _pool.TryTake(out var buffer) 
            ? buffer 
            : new byte[_bufferSize];
    }
    
    public void Return(byte[] buffer)
    {
        _pool.Add(buffer);
    }
}
```

**Usage:**
```csharp
var buffer = bufferPool.Rent();
try
{
    var received = await socket.ReceiveAsync(buffer, cancellationToken);
    ProcessData(buffer.AsSpan(0, received));
}
finally
{
    bufferPool.Return(buffer);
}
```

## Connection Lifecycle

### Connection Establishment

1. Client connects to TCP port 2593
2. `GameTcpServer` accepts connection
3. `GameNetworkSession` created
4. Session starts receive loop

### Connection Termination

1. Client disconnects (socket closed)
2. `GameNetworkSession.RunAsync` exits loop
3. Session disposed
4. Resources returned to pools

### Session State

```csharp
public enum SessionState
{
    Disconnected,
    Connected,
    InGame,
    Closing
}
```

## Security Considerations

### Rate Limiting

Prevent flood attacks:

```csharp
if (_packetsReceivedThisSecond > MaxPacketsPerSecond)
{
    Disconnect("Packet flood detected");
}
```

### Packet Validation

Validate all incoming data:

```csharp
if (packetId == 0x00 || packetId > 0xFF)
{
    Disconnect("Invalid packet ID");
}

if (reader.Remaining < expectedLength)
{
    Disconnect("Truncated packet");
}
```

### Connection Limits

Prevent resource exhaustion:

```csharp
if (_sessions.Count >= MaxConnections)
{
    client.Close();
    return;
}
```

## Performance Optimizations

### Zero-Allocation Design

- `SpanReader` / `SpanWriter` are `ref struct` (stack-only)
- Buffer pooling eliminates allocations
- No boxing/unboxing in hot paths

### Async I/O

- All socket operations are async
- No thread blocking on I/O
- Efficient use of thread pool

### Batch Processing

- Process multiple packets per game loop tick
- Reduce context switching overhead

## Monitoring

### Metrics Exposed

```
moongate_network_inbound_packets_total
moongate_network_outbound_packets_total
moongate_network_inbound_queue_depth
moongate_network_outbound_queue_depth
```

### Logging

```csharp
if (config.LogPacketData)
{
    logger.LogDebug("Packet 0x{PacketId:X2} received from {SessionId}", 
        packetId, session.Serial);
}
```

## Testing

### Unit Tests

```csharp
[Fact]
public void SpanReader_ReadUInt32_CorrectValue()
{
    var data = new byte[] { 0x01, 0x00, 0x00, 0x00 };
    var reader = new SpanReader(data);
    var value = reader.ReadUInt32();
    Assert.Equal(1u, value);
}
```

### Integration Tests

```csharp
[Fact]
public async Task TcpServer_AcceptsConnection()
{
    using var server = new GameTcpServer(...);
    await server.StartAsync(_cancellationToken);
    
    using var client = new TcpClient();
    await client.ConnectAsync("localhost", 2593);
    
    Assert.True(client.Connected);
}
```

## Next Steps

- **[Packet System](../networking/packets.md)** - Packet definitions and handlers
- **[Protocol Reference](../networking/protocol.md)** - UO protocol details
- **[Game Loop](game-loop.md)** - How packets are processed

---

**Previous**: [Architecture Overview](overview.md) | **Next**: [Game Loop](game-loop.md)
