# Protocol Reference

Complete reference for the Ultima Online protocol implementation in Moongate v2.

## Packet Structure

### Basic Packet Format

```
┌──────────┬──────────┬─────────────────────────────┐
│ PacketID │  Length  │          Data               │
│  1 byte  │ 2 bytes* │        Variable             │
└──────────┴──────────┴─────────────────────────────┘
* Only for variable-length packets
```

### Fixed-Length Packets

```csharp
// Example: Move Request (0x02)
// Length: 7 bytes
[0x02] [Sequence] [Direction] [X] [Y] [Z]
```

### Variable-Length Packets

```csharp
// Example: Speech Request (0xAD)
// Length: Specified in packet
[0xAD] [Length] [Type] [Hue] [Font] [Text...]
```

## Client → Server Packets

### 0x00 - Create Character

```csharp
[PacketHandler(0x00, "Create Character", Length = 104)]
public sealed class CreateCharacterHandler : IPacketHandler
{
    // Character name (30 chars)
    // Hair style, hair color
    // Facial hair style, facial hair color
    // Skin hue
    // Starting stats (Str, Dex, Int)
    // Starting skills (3 skills)
    // Starting location
    // Starting items
}
```

### 0x02 - Move Request

```csharp
[PacketHandler(0x02, "Move Request", Length = 7)]
public sealed class MoveRequestHandler : IPacketHandler
{
    public void Handle(SpanReader reader, GameNetworkSession session)
    {
        var sequence = reader.ReadByte();    // Movement sequence
        var direction = reader.ReadByte();   // Direction + running flag
        // Bit 7: Running flag
        // Bits 0-6: Direction (0-7)
    }
}
```

### 0x06 - Double Click

```csharp
[PacketHandler(0x06, "Double Click", Length = 5)]
public sealed class DoubleClickHandler : IPacketHandler
{
    public void Handle(SpanReader reader, GameNetworkSession session)
    {
        var serial = reader.ReadUInt32();  // Object serial
        // Use object (door, container, item, etc.)
    }
}
```

### 0x07 - Pick Up Item

```csharp
[PacketHandler(0x07, "Pick Up Item", Length = 7)]
public sealed class PickUpItemHandler : IPacketHandler
{
    public void Handle(SpanReader reader, GameNetworkSession session)
    {
        var serial = reader.ReadUInt32();  // Item serial
        var amount = reader.ReadUInt16();   // Amount to pick up
    }
}
```

### 0x08 - Drop Item

```csharp
[PacketHandler(0x08, "Drop Item", Length = 15)]
public sealed class DropItemHandler : IPacketHandler
{
    public void Handle(SpanReader reader, GameNetworkSession session)
    {
        var itemSerial = reader.ReadUInt32();    // Item serial
        var amount = reader.ReadUInt16();         // Amount
        var containerSerial = reader.ReadUInt32(); // Container serial
        var x = reader.ReadUInt16();              // X position
        var y = reader.ReadUInt16();              // Y position
        var z = reader.ReadByte();                // Z position (grid index for containers)
    }
}
```

### 0x12 - Use Skill

```csharp
[PacketHandler(0x12, "Use Skill", Length = -1)]
public sealed class UseSkillHandler : IPacketHandler
{
    public void Handle(SpanReader reader, GameNetworkSession session)
    {
        var length = reader.ReadUInt16();
        var skillId = reader.ReadByte();
        var serial = reader.ReadUInt32();  // Target serial (if applicable)
    }
}
```

### 0x34 - Get Player Status

```csharp
[PacketHandler(0x34, "Get Player Status", Length = 10)]
public sealed class GetPlayerStatusHandler : IPacketHandler
{
    public void Handle(SpanReader reader, GameNetworkSession session)
    {
        var serial = reader.ReadUInt32();  // Target serial
        var type = reader.ReadByte();       // Status type
    }
}
```

### 0x5D - Login Character

```csharp
[PacketHandler(0x5D, "Login Character", Length = 73)]
public sealed class LoginCharacterHandler : IPacketHandler
{
    public void Handle(SpanReader reader, GameNetworkSession session)
    {
        var serial = reader.ReadUInt32();  // Character serial
        // Character selection
    }
}
```

### 0x72 - Request War Mode

```csharp
[PacketHandler(0x72, "Request War Mode", Length = 5)]
public sealed class RequestWarModeHandler : IPacketHandler
{
    public void Handle(SpanReader reader, GameNetworkSession session)
    {
        var warmode = reader.ReadByte();   // 0 = peace, 1 = war
        var unknown = reader.ReadByte();   // Always 0x00
        var unknown2 = reader.ReadByte();  // Always 0x00
        var unknown3 = reader.ReadByte();  // Always 0x00
        var unknown4 = reader.ReadByte();  // Always 0x00
    }
}
```

### 0x80 - Login Request

```csharp
[PacketHandler(0x80, "Login Request", Length = 62)]
public sealed class LoginRequestHandler : IPacketHandler
{
    public void Handle(SpanReader reader, GameNetworkSession session)
    {
        var account = reader.ReadAscii(30);   // Account username
        var password = reader.ReadAscii(30);  // Account password
    }
}
```

### 0x83 - Delete Character

```csharp
[PacketHandler(0x83, "Delete Character", Length = 39)]
public sealed class DeleteCharacterHandler : IPacketHandler
{
    public void Handle(SpanReader reader, GameNetworkSession session)
    {
        var serial = reader.ReadUInt32();  // Character serial
        // Character deletion
    }
}
```

### 0xAD - Speech Request

```csharp
[PacketHandler(0xAD, "Speech Request", Length = -1)]
public sealed class SpeechRequestHandler : IPacketHandler
{
    public void Handle(SpanReader reader, GameNetworkSession session)
    {
        var length = reader.ReadUInt16();
        var type = reader.ReadByte();      // Speech type
        var hue = reader.ReadUInt16();     // Text hue
        var font = reader.ReadUInt16();    // Font ID
        var text = reader.ReadAscii(length - 9);  // Speech text
    }
}
```

### 0xB1 - Gump Menu Selection

```csharp
[PacketHandler(0xB1, "Gump Menu Selection", Length = -1)]
public sealed class GumpMenuSelectionHandler : IPacketHandler
{
    public void Handle(SpanReader reader, GameNetworkSession session)
    {
        var length = reader.ReadUInt16();
        var gumpId = reader.ReadUInt32();
        var serial = reader.ReadUInt32();
        var type = reader.ReadUInt16();
        var switchCount = reader.ReadUInt16();
        // Switches...
        var textCount = reader.ReadUInt16();
        // Text entries...
    }
}
```

### 0xB8 - Request Character Profile

```csharp
[PacketHandler(0xB8, "Request/Char Profile", Length = -1)]
public sealed class RequestCharacterProfileHandler : IPacketHandler
{
    public void Handle(SpanReader reader, GameNetworkSession session)
    {
        var length = reader.ReadUInt16();
        var type = reader.ReadByte();
        var serial = reader.ReadUInt32();
    }
}
```

### 0xBF - Extended Command

```csharp
[PacketHandler(0xBF, "Extended Command", Length = -1)]
public sealed class ExtendedCommandHandler : IPacketHandler
{
    public void Handle(SpanReader reader, GameNetworkSession session)
    {
        var length = reader.ReadUInt16();
        var subCommand = reader.ReadUInt16();
        
        // Route to sub-command handler
        HandleSubCommand(subCommand, reader, session);
    }
}
```

## Server → Client Packets

### 0x11 - Status Bar Info

```csharp
public sealed class StatusBarInfoPacket : IOutgoingPacket
{
    public int PacketId => 0x11;
    public int Length => -1;
    
    // Mobile serial
    // Mobile name
    // Hits, HitsMax
    // Name change flag
    // Flags (warmode, poisoned, etc.)
    // Notoriety
    // Stats (Str, Dex, Int)
    // Resistances
    // Skills
}
```

### 0x1C - Send Speech

```csharp
public sealed class SendSpeechPacket : IOutgoingPacket
{
    public int PacketId => 0x1C;
    public int Length => -1;
    
    // Source serial
    // Source name
    // Speech text
    // Speech type
    // Hue
    // Font
    // Language
}
```

### 0x20 - Draw Game Player

```csharp
public sealed class DrawGamePlayerPacket : IOutgoingPacket
{
    public int PacketId => 0x20;
    public int Length => 19;
    
    // Mobile serial
    // Body ID
    // Hue
    // Flags
    // X, Y, Z
    // Direction
    // Notoriety
}
```

### 0x24 - Draw Container

```csharp
public sealed class DrawContainerPacket : IOutgoingPacket
{
    public int PacketId => 0x24;
    public int Length => 9;
    
    // Container serial
    // Container graphic ID
    // Unknown (usually 0)
    // Container capacity
}
```

### 0x25 - Add Item To Container

```csharp
public sealed class AddItemToContainerPacket : IOutgoingPacket
{
    public int PacketId => 0x25;
    public int Length => 20;
    
    // Item serial
    // Item graphic ID
    // Unknown (usually 0)
    // Amount
    // X, Y position
    // Grid index (for containers)
    // Container serial
    // Item hue
}
```

### 0x2C - Resurrection Menu

```csharp
public sealed class ResurrectionMenuPacket : IOutgoingPacket
{
    public int PacketId => 0x2C;
    public int Length => 2;
    
    // Action type
    // 0x00 = Resurrect
    // 0x01 = Return as ghost
    // 0x02 = Help
}
```

### 0x3A - Send Skills

```csharp
public sealed class SendSkillsPacket : IOutgoingPacket
{
    public int PacketId => 0x3A;
    public int Length => -1;
    
    // Skill count
    // For each skill:
    //   Skill ID
    //   Skill value
    //   Skill cap
    //   Lock type (up/down/down)
}
```

### 0x54 - Play Sound Effect

```csharp
public sealed class PlaySoundEffectPacket : IOutgoingPacket
{
    public int PacketId => 0x54;
    public int Length => 12;
    
    // Mode (0x00 = one shot, 0x01 = repeat)
    // Sound ID
    // Unknown (usually 0x0000)
    // X, Y, Z position
}
```

### 0x55 - Login Complete

```csharp
public sealed class LoginCompletePacket : IOutgoingPacket
{
    public int PacketId => 0x55;
    public int Length => 1;
    
    // No data - signals login complete
}
```

### 0x77 - Update Player

```csharp
public sealed class UpdatePlayerPacket : IOutgoingPacket
{
    public int PacketId => 0x77;
    public int Length => 17;
    
    // Mobile serial
    // Body ID
    // Hue
    // Flags
    // X, Y, Z
    // Direction
    // Notoriety
}
```

### 0x78 - Draw Object

```csharp
public sealed class DrawObjectPacket : IOutgoingPacket
{
    public int PacketId => 0x78;
    public int Length => -1;
    
    // Mobile serial
    // Body ID
    // Hue
    // Flags
    // X, Y, Z
    // Direction
    // Notoriety
    // Equipment (serial + item ID + hue for each)
    // End of equipment marker (0x0000)
}
```

### 0x88 - Open Paperdoll

```csharp
public sealed class OpenPaperdollPacket : IOutgoingPacket
{
    public int PacketId => 0x88;
    public int Length => 66;
    
    // Mobile serial
    // Paperdoll graphic (0x1E for male, 0x19 for female)
    // Mobile name
    // Flags
    // Mobile title
}
```

### 0x89 - Corpse Clothing

```csharp
public sealed class CorpseClothingPacket : IOutgoingPacket
{
    public int PacketId => 0x89;
    public int Length => -1;
    
    // Corpse serial
    // Equipment list
}
```

### 0xB0 - Send Gump Menu Dialog

```csharp
public sealed class SendGumpMenuDialogPacket : IOutgoingPacket
{
    public int PacketId => 0xB0;
    public int Length => -1;
    
    // Player serial
    // Gump ID
    // Gump data (JSON-like format)
}
```

### 0xBF:0x06 - Client Version

```csharp
public sealed class ClientVersionSubPacket : IOutgoingPacket
{
    public int SubCommand => 0x06;
    
    // Requests client version from client
}
```

## Extended Commands (0xBF)

### Sub-Command Reference

| SubID | Name | Length | Description |
|-------|------|--------|-------------|
| 0x01 | Disarm Request | 3 | Request disarm action |
| 0x02 | Stun Request | 3 | Request stun action |
| 0x04 | Character Query | 5 | Query character info |
| 0x06 | Client Version | 1 | Request client version |
| 0x09 | Help Request | 1 | Request help menu |
| 0x10 | Cast Spell | 3 | Cast spell from book |
| 0x12 | Target Response | 19 | Target cursor response |
| 0x14 | Quest Arrow Click | 3 | Quest arrow interaction |
| 0x1A | Toggle Gargoyle Flying | 1 | Toggle flying (SA) |
| 0x1E | Set Update Range | 3 | Set client view range |
| 0x24 | Client Login Complete | 1 | KR login complete |
| 0x27 | UO3D Macro | - | UO3D macro execution |
| 0x28 | UO3D Pathfinding | 7 | Pathfinding request |

## Next Steps

- **[Packet System](packets.md)** - Packet handling implementation
- **[Network Architecture](../architecture/network.md)** - Network system design
- **[Scripting API](../scripting/api.md)** - Scripting reference

---

**Previous**: [Packet System](packets.md) | **Next**: [API Reference](../scripting/api.md)
