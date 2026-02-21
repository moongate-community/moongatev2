# Packet System

Comprehensive guide to Moongate v2's packet handling system.

## Overview

Moongate v2 implements a type-safe, source-generated packet system for the Ultima Online protocol.

Key features:

- Attribute-based packet registration
- Zero-allocation parsing with `Span<T>`
- Source-generated packet registry
- Strict inbound/outbound separation

## Packet Registration

### Attribute-Based Registration

Packets are registered using attributes:

```csharp
[PacketHandler(0x02, "Move Request", Length = 7)]
public sealed class MoveRequestHandler : IPacketHandler
{
    public void Handle(SpanReader reader, GameNetworkSession session)
    {
        var sequence = reader.ReadByte();
        var direction = reader.ReadByte();
        
        // Process movement request
    }
}
```

### PacketHandler Attribute

```csharp
[AttributeUsage(AttributeTargets.Class)]
public sealed class PacketHandlerAttribute : Attribute
{
    public int PacketId { get; }
    public string Name { get; }
    public int Length { get; }  // -1 for variable-length
    
    public PacketHandlerAttribute(int packetId, string name, int length = -1)
    {
        PacketId = packetId;
        Name = name;
        Length = length;
    }
}
```

### Source Generator

The source generator creates the packet registry at compile time:

```csharp
// Generated code (do not edit)
namespace Moongate.Network.Packets.Registry;

public static partial class PacketRegistry
{
    public static readonly IPacketHandler[] Handlers = new IPacketHandler[256]
    {
        [0x02] = new MoveRequestHandler(),
        [0x06] = new DoubleClickHandler(),
        [0x80] = new LoginRequestHandler(),
        // ... all registered handlers
    };
    
    public static readonly string[] Names = new string[256]
    {
        [0x02] = "Move Request",
        [0x06] = "Double Click",
        [0x80] = "Login Request",
        // ...
    };
}
```

## Packet Types

### Fixed-Length Packets

```csharp
[PacketHandler(0x06, "Double Click", Length = 5)]
public sealed class DoubleClickHandler : IPacketHandler
{
    public void Handle(SpanReader reader, GameNetworkSession session)
    {
        // Always exactly 5 bytes
        var serial = reader.ReadUInt32();
        
        logger.LogDebug("Double click on 0x{Serial:X8}", serial);
    }
}
```

### Variable-Length Packets

```csharp
[PacketHandler(0xAD, "Speech Request", Length = -1)]
public sealed class SpeechRequestHandler : IPacketHandler
{
    public void Handle(SpanReader reader, GameNetworkSession session)
    {
        // Length specified in packet header
        var length = reader.ReadUInt16();  // Includes length bytes
        var type = reader.ReadByte();
        var hue = reader.ReadUInt16();
        var font = reader.ReadUInt16();
        var text = reader.ReadAscii(length - 9);
        
        logger.LogDebug("Speech: {Text}", text);
    }
}
```

### Extended Packets (0xBF)

```csharp
[PacketHandler(0xBF, "Extended Command", Length = -1)]
public sealed class ExtendedCommandHandler : IPacketHandler
{
    public void Handle(SpanReader reader, GameNetworkSession session)
    {
        var length = reader.ReadUInt16();
        var subCommand = reader.ReadUInt16();
        
        // Route to sub-command handler
        _extendedHandlerRegistry.Handle(subCommand, reader, session);
    }
}
```

## Packet Handlers

### IPacketHandler Interface

```csharp
public interface IPacketHandler
{
    int PacketId { get; }
    int Length { get; }
    string Name { get; }
    void Handle(SpanReader reader, GameNetworkSession session);
}
```

### Base Handler Class

```csharp
public abstract class PacketHandler : IPacketHandler
{
    public int PacketId { get; }
    public int Length { get; }
    public string Name { get; }
    
    protected PacketHandler(int packetId, string name, int length = -1)
    {
        PacketId = packetId;
        Name = name;
        Length = length;
    }
    
    public abstract void Handle(SpanReader reader, GameNetworkSession session);
}
```

## Packet I/O

### SpanReader

Zero-allocation binary reader:

```csharp
public ref struct SpanReader
{
    private ReadOnlySpan<byte> _span;
    
    public SpanReader(ReadOnlySpan<byte> data)
    {
        _span = data;
    }
    
    public byte ReadByte()
    {
        var value = _span[0];
        _span = _span.Slice(1);
        return value;
    }
    
    public ushort ReadUInt16()
    {
        var value = BinaryPrimitives.ReadUInt16LittleEndian(_span);
        _span = _span.Slice(2);
        return value;
    }
    
    public uint ReadUInt32()
    {
        var value = BinaryPrimitives.ReadUInt32LittleEndian(_span);
        _span = _span.Slice(4);
        return value;
    }
    
    public string ReadAscii(int length)
    {
        var value = Encoding.ASCII.GetString(_span.Slice(0, length));
        _span = _span.Slice(length);
        return value;
    }
    
    public int Remaining => _span.Length;
}
```

### SpanWriter

Zero-allocation binary writer:

```csharp
public ref struct SpanWriter
{
    private Span<byte> _span;
    private int _position;
    
    public SpanWriter(Span<byte> buffer)
    {
        _span = buffer;
        _position = 0;
    }
    
    public void Write(byte value)
    {
        _span[_position++] = value;
    }
    
    public void Write(ushort value)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(_span.Slice(_position), value);
        _position += 2;
    }
    
    public void Write(uint value)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(_span.Slice(_position), value);
        _position += 4;
    }
    
    public void WriteAscii(string value)
    {
        Encoding.ASCII.GetBytes(value, _span.Slice(_position));
        _position += value.Length;
    }
    
    public int Position => _position;
}
```

## Outbound Packets

### IOutgoingPacket Interface

```csharp
public interface IOutgoingPacket
{
    int PacketId { get; }
    int Length { get; }
    void Serialize(SpanWriter writer);
}
```

### Example Outbound Packet

```csharp
public sealed class WelcomePacket : IOutgoingPacket
{
    public int PacketId => 0x55;
    public int Length => 5;
    
    public void Serialize(SpanWriter writer)
    {
        writer.Write((byte)PacketId);
        writer.Write((ushort)Length);
    }
}

public sealed class DrawObjectPacket : IOutgoingPacket
{
    public int PacketId => 0x78;
    public int Length => -1;  // Variable
    
    private readonly Serial _serial;
    private readonly int _body;
    private readonly ushort _hue;
    private readonly MobileFlags _flags;
    private readonly Point3D _position;
    private readonly Direction _direction;
    
    public DrawObjectPacket(Mobile mobile)
    {
        _serial = mobile.Serial;
        _body = mobile.Body;
        _hue = mobile.Hue;
        _flags = mobile.Flags;
        _position = mobile.Position;
        _direction = mobile.Direction;
    }
    
    public void Serialize(SpanWriter writer)
    {
        var lengthPosition = writer.Position;
        writer.Write((byte)PacketId);
        writer.Write((ushort)0);  // Placeholder for length
        
        writer.Write(_serial);
        writer.Write((ushort)_body);
        writer.Write(_hue);
        writer.Write((byte)_flags);
        writer.Write((ushort)_position.X);
        writer.Write((ushort)_position.Y);
        writer.Write((sbyte)_position.Z);
        writer.Write((byte)_direction);
        writer.Write((byte)0);  // Notoriety
        
        // Calculate and write actual length
        var actualLength = writer.Position;
        BinaryPrimitives.WriteUInt16LittleEndian(
            writer._span.Slice(lengthPosition + 1), 
            (ushort)actualLength
        );
    }
}
```

## Packet Queue

### IOutgoingPacketQueue

```csharp
public interface IOutgoingPacketQueue
{
    void Enqueue<TPacket>(TPacket packet, NetworkSession session) 
        where TPacket : IOutgoingPacket;
    void Flush();
}
```

### Implementation

```csharp
public sealed class OutgoingPacketQueue : IOutgoingPacketQueue
{
    private readonly Queue<(IOutgoingPacket Packet, NetworkSession Session)> _queue = new();
    
    public void Enqueue<TPacket>(TPacket packet, NetworkSession session) 
        where TPacket : IOutgoingPacket
    {
        _queue.Enqueue((packet, session));
    }
    
    public void Flush()
    {
        while (_queue.Count > 0)
        {
            var (packet, session) = _queue.Dequeue();
            
            var buffer = ArrayPool<byte>.Shared.Rent(4096);
            try
            {
                var writer = new SpanWriter(buffer.AsSpan());
                packet.Serialize(writer);
                session.Send(buffer.AsSpan(0, writer.Position));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
```

## Packet Logging

### Debug Logging

```csharp
if (config.LogPacketData)
{
    logger.LogDebug("← Packet 0x{PacketId:X2} ({Name}) from {Session}", 
        packetId, 
        PacketRegistry.Names[packetId] ?? "Unknown", 
        session.Serial);
}
```

### Packet Trace

```csharp
public void LogPacket(ReadOnlySpan<byte> packet, bool inbound)
{
    var packetId = packet[0];
    var direction = inbound ? "←" : "→";
    
    var hexBuilder = new StringBuilder();
    for (int i = 0; i < Math.Min(packet.Length, 32); i++)
    {
        hexBuilder.Append($"{packet[i]:X2} ");
    }
    
    logger.LogTrace("{Direction} Packet 0x{PacketId:X2}: {Hex}", 
        direction, packetId, hexBuilder.ToString());
}
```

## Protocol Reference

### Common Packet IDs

| ID | Name | Length | Direction |
|----|------|--------|-----------|
| 0x00 | Create Character | 104 | C→S |
| 0x02 | Move Request | 7 | C→S |
| 0x06 | Double Click | 5 | C→S |
| 0x11 | Status Bar Info | Variable | S→C |
| 0x1C | Send Speech | Variable | S→C |
| 0x20 | Draw Game Player | 19 | S→C |
| 0x55 | Login Complete | 1 | S→C |
| 0x78 | Draw Object | Variable | S→C |
| 0x80 | Login Request | 62 | C→S |
| 0x88 | Open Paperdoll | 66 | S→C |
| 0xAD | Speech Request | Variable | C→S |
| 0xBF | Extended Command | Variable | Both |

### Extended Commands (0xBF)

| SubID | Name |
|-------|------|
| 0x01 | Disarm Request |
| 0x02 | Stun Request |
| 0x04 | Character Query |
| 0x06 | Client Version |
| 0x09 | Help Request |
| 0x10 | Cast Spell |
| 0x12 | Target Response |

## Error Handling

### Invalid Packets

```csharp
public void HandlePacket(ReadOnlySpan<byte> packet, GameNetworkSession session)
{
    var packetId = packet[0];
    
    if (packetId >= PacketRegistry.Handlers.Length)
    {
        logger.LogWarning("Invalid packet ID 0x{PacketId:X2}", packetId);
        session.Disconnect("Invalid packet");
        return;
    }
    
    var handler = PacketRegistry.Handlers[packetId];
    if (handler == null)
    {
        logger.LogWarning("Unhandled packet ID 0x{PacketId:X2}", packetId);
        return;  // Don't disconnect for unhandled packets
    }
    
    if (handler.Length != -1 && packet.Length != handler.Length)
    {
        logger.LogWarning("Packet 0x{PacketId:X2} length mismatch: expected {Expected}, got {Actual}", 
            packetId, handler.Length, packet.Length);
        session.Disconnect("Protocol violation");
        return;
    }
    
    handler.Handle(new SpanReader(packet), session);
}
```

## Testing

### Unit Tests

```csharp
[Fact]
public void MoveRequestHandler_ValidPacket_ProcessesMovement()
{
    var handler = new MoveRequestHandler();
    var data = new byte[] { 0x02, 0x01, 0x05, 0x00, 0x00, 0x00, 0x00 };
    var reader = new SpanReader(data);
    var session = CreateMockSession();
    
    handler.Handle(reader, session);
    
    // Verify movement was processed
    mockMobileService.Verify(m => m.Move(It.IsAny<Direction>(), It.IsAny<byte>()));
}
```

### Packet Serialization Tests

```csharp
[Fact]
public void DrawObjectPacket_Serialize_CorrectFormat()
{
    var mobile = CreateTestMobile();
    var packet = new DrawObjectPacket(mobile);
    var buffer = new byte[256];
    var writer = new SpanWriter(buffer);
    
    packet.Serialize(writer);
    
    // Verify packet structure
    Assert.Equal(0x78, buffer[0]);
    // ... verify other fields
}
```

## Next Steps

- **[Protocol Reference](protocol.md)** - Complete UO protocol details
- **[Network System](../architecture/network.md)** - Network architecture
- **[Event System](../architecture/events.md)** - Event-driven packet handling

---

**Previous**: [Networking Overview](../architecture/network.md) | **Next**: [Protocol Reference](protocol.md)
