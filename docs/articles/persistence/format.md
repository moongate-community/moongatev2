# Data Format

Binary serialization format used in Moongate v2 persistence.

## Overview

Moongate v2 uses **MemoryPack** for binary serialization, providing:

- Zero-copy deserialization
- High performance (faster than MessagePack, Protobuf)
- AOT-compatible (no reflection)
- Compact binary format

## Snapshot Format

### File Structure

```
world.snapshot.bin:
┌─────────────────────────────────┐
│   MemoryPack Header (4 bytes)   │
├─────────────────────────────────┤
│   WorldSnapshot (serialized)    │
├─────────────────────────────────┤
│   Checksum (4 bytes)            │
└─────────────────────────────────┘
```

### WorldSnapshot Structure

```csharp
[MemoryPackable]
public partial class WorldSnapshot
{
    public DateTime Timestamp { get; init; }
    public long Checksum { get; init; }
    public List<AccountEntity> Accounts { get; init; }
    public List<MobileEntity> Mobiles { get; init; }
    public List<ItemEntity> Items { get; init; }
    public Dictionary<Serial, Serial> Relationships { get; init; }
}
```

### Binary Layout

```
Offset  Size  Field
──────  ────  ─────
0       4     Magic number (0x4D504B57 = "MPKW")
4       8     Timestamp (ticks)
12      8     Checksum
20      4     Account count
24      N     Account entities
24+N    4     Mobile count
28+N    M     Mobile entities
28+N+M  4     Item count
32+N+M  K     Item entities
```

## Journal Format

### File Structure

```
world.journal.bin:
┌─────────────────────────────────┐
│   Journal Entry 1               │
├─────────────────────────────────┤
│   Journal Entry 2               │
├─────────────────────────────────┤
│   ...                           │
├─────────────────────────────────┤
│   Journal Entry N               │
└─────────────────────────────────┘
```

### Journal Entry Structure

```csharp
[MemoryPackable]
public partial struct JournalEntry
{
    public long Timestamp;                      // 8 bytes
    public JournalOperationType Operation;      // 1 byte
    public byte[] Data;                         // Variable
    public uint Checksum;                       // 4 bytes
}
```

### Operation Types

```csharp
public enum JournalOperationType : byte
{
    None = 0,
    AccountCreated = 1,
    AccountDeleted = 2,
    AccountUpdated = 3,
    MobileCreated = 4,
    MobileDeleted = 5,
    MobileUpdated = 6,
    MobilePositionUpdated = 7,
    ItemCreated = 8,
    ItemDeleted = 9,
    ItemUpdated = 10,
    ItemMoved = 11,
    ItemEquipped = 12,
    ItemUnequipped = 13
}
```

## Entity Formats

### Account Entity

```csharp
[MemoryPackable]
public partial struct AccountEntity
{
    public Serial Serial;           // 4 bytes
    public string Username;         // Variable (length-prefixed)
    public string PasswordHash;     // Variable
    public AccountFlags Flags;      // 4 bytes
    public DateTime CreatedAt;      // 8 bytes
    public DateTime LastLogin;      // 8 bytes
    public List<Serial> Characters; // 4 bytes count + N*4 bytes
}
```

### Mobile Entity

```csharp
[MemoryPackable]
public partial struct UOMobileEntity
{
    public Serial Serial;                   // 4 bytes
    public string Name;                     // Variable
    public int Body;                        // 4 bytes
    public int Hue;                         // 4 bytes
    public Point3D Position;                // 12 bytes (X, Y, Z)
    public Map Facet;                       // 4 bytes
    public MobileFlags Flags;               // 4 bytes
    public Direction Direction;             // 1 byte
    public int Hits;                        // 4 bytes
    public int HitsMax;                     // 4 bytes
    public int Stamina;                     // 4 bytes
    public int StaminaMax;                  // 4 bytes
    public int Mana;                        // 4 bytes
    public int ManaMax;                     // 4 bytes
    public Serial BackpackId;               // 4 bytes
    public List<Serial> EquippedItemIds;    // 4 bytes count + N*4 bytes
    public Dictionary<int, int> Skills;     // 4 bytes count + N*8 bytes
    public Serial OwnerAccount;             // 4 bytes
}
```

### Item Entity

```csharp
[MemoryPackable]
public partial struct UOItemEntity
{
    public Serial Serial;                   // 4 bytes
    public int ItemId;                      // 4 bytes
    public int Amount;                      // 4 bytes
    public int Hue;                         // 4 bytes
    public Point3D Position;                // 12 bytes
    public Map Facet;                       // 4 bytes
    public Serial? ParentContainerId;       // 4 bytes (nullable)
    public ContainerPosition? ContainerPos; // 8 bytes (nullable)
    public Serial? EquippedMobileId;        // 4 bytes (nullable)
    public Layer EquippedLayer;             // 1 byte (nullable)
    public bool IsMovable;                  // 1 byte
    public bool IsContainer;                // 1 byte
    public List<Serial> Contents;           // 4 bytes count + N*4 bytes
    public string? Name;                    // Variable (nullable)
}
```

## Serialization

### Writing Snapshot

```csharp
public async Task SaveSnapshotAsync(CancellationToken cancellationToken)
{
    var snapshot = new WorldSnapshot
    {
        Timestamp = DateTime.UtcNow,
        Accounts = _accountRepository.GetAll(),
        Mobiles = _mobileRepository.GetAll(),
        Items = _itemRepository.GetAll()
    };
    
    using var stream = File.Create(_snapshotPath);
    
    // Serialize
    await MemoryPackSerializer.SerializeAsync(stream, snapshot, cancellationToken);
    
    // Write checksum
    var checksum = CalculateChecksum(snapshot);
    await stream.WriteAsync(BitConverter.GetBytes(checksum), cancellationToken);
    
    await stream.FlushAsync(cancellationToken);
}
```

### Reading Snapshot

```csharp
public async Task<WorldSnapshot?> LoadSnapshotAsync(CancellationToken cancellationToken)
{
    using var stream = File.OpenRead(_snapshotPath);
    
    // Read magic number
    var magic = new byte[4];
    await stream.ReadAsync(magic, cancellationToken);
    
    if (BitConverter.ToUInt32(magic, 0) != 0x4D504B57)
    {
        logger.LogError("Invalid snapshot file");
        return null;
    }
    
    // Deserialize
    var snapshot = await MemoryPackSerializer.DeserializeAsync<WorldSnapshot>(
        stream, 
        cancellationToken: cancellationToken
    );
    
    // Verify checksum
    var storedChecksum = new byte[4];
    await stream.ReadAsync(storedChecksum, cancellationToken);
    
    var calculatedChecksum = CalculateChecksum(snapshot);
    if (calculatedChecksum != BitConverter.ToUInt32(storedChecksum, 0))
    {
        logger.LogError("Snapshot checksum mismatch");
        return null;
    }
    
    return snapshot;
}
```

### Appending to Journal

```csharp
public async Task AppendAsync(JournalEntry entry, CancellationToken cancellationToken)
{
    // Calculate checksum
    entry.Checksum = CalculateChecksum(entry);
    
    using var stream = File.Open(_journalPath, FileMode.Append, FileAccess.Write);
    
    // Append entry
    await MemoryPackSerializer.SerializeAsync(stream, entry, cancellationToken);
    
    // Flush to disk
    await stream.FlushAsync(cancellationToken);
}
```

## Compression

### Optional Compression

```csharp
public async Task SaveCompressedSnapshotAsync(WorldSnapshot snapshot)
{
    using var memoryStream = new MemoryStream();
    
    // Serialize to memory
    await MemoryPackSerializer.SerializeAsync(memoryStream, snapshot);
    memoryStream.Position = 0;
    
    // Compress
    using var fileStream = File.Create(_snapshotPath + ".gz");
    using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
    await memoryStream.CopyToAsync(gzipStream);
}
```

## Versioning

### Schema Version

```csharp
[MemoryPackable]
public partial class WorldSnapshot
{
    public int SchemaVersion { get; init; } = 1;  // Schema version
    // ... other fields
}
```

### Migration

```csharp
public async Task MigrateSnapshotAsync(string sourcePath, string destPath)
{
    // Load old snapshot
    var oldSnapshot = await LoadOldSnapshotAsync(sourcePath);
    
    // Convert to new format
    var newSnapshot = new WorldSnapshot
    {
        SchemaVersion = 2,
        Timestamp = oldSnapshot.Timestamp,
        Accounts = ConvertAccounts(oldSnapshot.Accounts),
        Mobiles = ConvertMobiles(oldSnapshot.Mobiles),
        Items = ConvertItems(oldSnapshot.Items)
    };
    
    // Save new snapshot
    await SaveSnapshotAsync(newSnapshot);
}
```

## Checksum Algorithm

### CRC32 Implementation

```csharp
private static readonly uint[] Crc32Table = GenerateCrc32Table();

private uint CalculateChecksum<T>(T value)
{
    var data = MemoryPackSerializer.Serialize(value);
    uint crc = 0xFFFFFFFF;
    
    foreach (var b in data)
    {
        crc = (crc >> 8) ^ Crc32Table[(crc ^ b) & 0xFF];
    }
    
    return crc ^ 0xFFFFFFFF;
}
```

## Next Steps

- **[Repositories](repositories.md)** - Data access patterns
- **[Persistence Overview](overview.md)** - System architecture
- **[Network System](../architecture/network.md)** - Network serialization

---

**Previous**: [Persistence Overview](overview.md) | **Next**: [Repositories](repositories.md)
