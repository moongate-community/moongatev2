# Persistence System

Moongate v2 uses a lightweight file-based persistence model with snapshot + journal architecture.

## Overview

The persistence system provides:

- **Snapshot files** for full world state checkpoints
- **Append-only journals** for incremental changes
- **MemoryPack serialization** for compact binary format
- **Checksum validation** for data integrity
- **Thread-safe repositories** for concurrent access

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                   Persistence System                         │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐   │
│  │ Repositories │───▶│  Journal     │───▶│  Snapshot    │   │
│  │ (Thread-safe)│    │  (Append)    │    │  (Checkpoint)│   │
│  └──────────────┘    └──────────────┘    └──────────────┘   │
│         │                    │                    │          │
│         │                    ▼                    ▼          │
│    ┌────┴────┐    ┌──────────────┐    ┌──────────────┐      │
│    │ Memory  │    │  Checksum    │    │  Compression │      │
│    │  Pack   │    │  Validation  │    │  (Optional)  │      │
│    └─────────┘    └──────────────┘    └──────────────┘      │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## Storage Structure

```
save/
├── world.snapshot.bin       # Full world state snapshot
├── world.journal.bin        # Append-only operation journal
├── world.journal.bin.checksum  # Journal checksum
└── snapshots/
    ├── snapshot-20240101-120000.bin
    ├── snapshot-20240101-180000.bin
    └── ...
```

## Snapshot System

### What is a Snapshot?

A snapshot is a complete binary serialization of the world state at a point in time.

### Snapshot Contents

```csharp
public sealed class WorldSnapshot
{
    public DateTime Timestamp { get; init; }
    public List<AccountEntity> Accounts { get; init; }
    public List<MobileEntity> Mobiles { get; init; }
    public List<ItemEntity> Items { get; init; }
    public long Checksum { get; init; }
}
```

### Creating Snapshots

```csharp
public async Task SaveSnapshotAsync(CancellationToken cancellationToken)
{
    using var stream = File.Create(_snapshotPath);
    
    var snapshot = new WorldSnapshot
    {
        Timestamp = DateTime.UtcNow,
        Accounts = _accountRepository.GetAll(),
        Mobiles = _mobileRepository.GetAll(),
        Items = _itemRepository.GetAll()
    };
    
    // Serialize with MemoryPack
    await MemoryPackSerializer.SerializeAsync(stream, snapshot, cancellationToken);
    
    logger.LogInformation("Snapshot saved: {Path}", _snapshotPath);
}
```

### Loading Snapshots

```csharp
public async Task LoadSnapshotAsync(CancellationToken cancellationToken)
{
    using var stream = File.OpenRead(_snapshotPath);
    
    var snapshot = await MemoryPackSerializer.DeserializeAsync<WorldSnapshot>(
        stream, 
        cancellationToken: cancellationToken
    );
    
    // Restore world state
    _accountRepository.Restore(snapshot.Accounts);
    _mobileRepository.Restore(snapshot.Mobiles);
    _itemRepository.Restore(snapshot.Items);
    
    logger.LogInformation("Snapshot loaded: {Count} accounts, {Mobiles} mobiles", 
        snapshot.Accounts.Count, snapshot.Mobiles.Count);
}
```

## Journal System

### What is a Journal?

A journal is an append-only log of operations that modify the world state.

### Journal Operations

```csharp
public enum JournalOperationType : byte
{
    AccountCreated = 1,
    AccountDeleted = 2,
    MobileCreated = 3,
    MobileDeleted = 4,
    MobileUpdated = 5,
    ItemCreated = 6,
    ItemDeleted = 7,
    ItemUpdated = 8
}
```

### Journal Entry Structure

```csharp
[MemoryPackable]
public partial struct JournalEntry
{
    public long Timestamp;
    public JournalOperationType Operation;
    public byte[] Data;  // Serialized operation data
    public uint Checksum;  // Per-entry checksum
}
```

### Appending to Journal

```csharp
public async Task AppendAsync(JournalEntry entry, CancellationToken cancellationToken)
{
    // Calculate checksum
    entry.Checksum = CalculateChecksum(entry);
    
    // Append to journal file
    using var stream = File.Open(_journalPath, FileMode.Append, FileAccess.Write);
    await MemoryPackSerializer.SerializeAsync(stream, entry, cancellationToken);
    
    // Flush to disk
    await stream.FlushAsync(cancellationToken);
}

// Usage
await _persistenceService.AppendAsync(new JournalEntry
{
    Timestamp = Stopwatch.GetTimestamp(),
    Operation = JournalOperationType.MobileCreated,
    Data = MemoryPackSerializer.Serialize(mobile)
}, cancellationToken);
```

### Replay Journal

```csharp
public async Task ReplayJournalAsync(CancellationToken cancellationToken)
{
    if (!File.Exists(_journalPath))
    {
        logger.LogInformation("No journal to replay");
        return;
    }
    
    using var stream = File.OpenRead(_journalPath);
    
    while (stream.Position < stream.Length)
    {
        var entry = await MemoryPackSerializer.DeserializeAsync<JournalEntry>(stream, cancellationToken);
        
        // Validate checksum
        if (!ValidateChecksum(entry))
        {
            logger.LogWarning("Journal entry checksum mismatch at position {Position}", stream.Position);
            break;  // Stop at corrupted entry
        }
        
        // Apply operation
        await ApplyJournalEntryAsync(entry, cancellationToken);
    }
    
    logger.LogInformation("Journal replayed");
}
```

## Repositories

### Thread-Safe Access

```csharp
public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Serial serial, CancellationToken cancellationToken);
    Task<Account?> GetByUsernameAsync(string username, CancellationToken cancellationToken);
    Task SaveAsync(Account account, CancellationToken cancellationToken);
    Task DeleteAsync(Serial serial, CancellationToken cancellationToken);
    IQueryResult<Account> QueryAsync(Func<IQueryable<Account>, IQueryable<Account>> query);
}
```

### Query Support

```csharp
// Query accounts
var accounts = await _accountRepository.QueryAsync(q =>
    q.Where(a => a.CreatedAt > DateTime.UtcNow.AddDays(-7))
     .OrderByDescending(a => a.LastLogin)
     .Take(10)
);

// Query mobiles
var mobiles = await _mobileRepository.QueryAsync(q =>
    q.Where(m => m.Position.Map == Map.Britannia)
     .Where(m => m.Position.X > 1000 && m.Position.X < 2000)
);
```

### Immutable Snapshots

Repositories work on immutable snapshots for queries:

```csharp
public IQueryResult<T> QueryAsync<T>(Func<IQueryable<T>, IQueryable<T>> query)
{
    // Get immutable snapshot
    var snapshot = _snapshotManager.GetCurrentSnapshot<T>();
    
    // Execute query on snapshot
    var result = query(snapshot.AsQueryable()).ToList();
    
    return new QueryResult<T>(result);
}
```

## Data Integrity

### Checksum Validation

```csharp
private uint CalculateChecksum(JournalEntry entry)
{
    using var hasher = new Crc32();
    hasher.Append(entry.Data);
    return hasher.GetCurrentHashAsUInt32();
}

private bool ValidateChecksum(JournalEntry entry)
{
    var calculated = CalculateChecksum(entry);
    return calculated == entry.Checksum;
}
```

### Corruption Recovery

```csharp
public async Task RecoverFromCorruptionAsync()
{
    // Find last valid snapshot
    var lastValidSnapshot = FindLastValidSnapshot();
    
    // Load snapshot
    await LoadSnapshotAsync(lastValidSnapshot);
    
    // Replay journal up to corruption point
    await ReplayJournalUpToAsync(lastValidSnapshot.Timestamp);
    
    logger.LogInformation("Recovery completed from {Snapshot}", lastValidSnapshot);
}
```

## Configuration

### Persistence Settings

```json
{
  "persistence": {
    "enabled": true,
    "snapshotIntervalMinutes": 5,
    "journalFlushIntervalSeconds": 30,
    "compressionEnabled": true,
    "maxJournalSizeMB": 100,
    "maxSnapshots": 10
  }
}
```

### Automatic Snapshots

```csharp
public async Task StartAutoSnapshotAsync(CancellationToken cancellationToken)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        await Task.Delay(TimeSpan.FromMinutes(_config.SnapshotIntervalMinutes), cancellationToken);
        
        try
        {
            await SaveSnapshotAsync(cancellationToken);
            await RotateJournalsAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Auto-snapshot failed");
        }
    }
}
```

## Performance

### Optimization Strategies

1. **Batch Operations**
   - Group multiple changes into single journal entry
   - Reduce disk I/O frequency

2. **Async I/O**
   - All file operations are async
   - Non-blocking for game loop

3. **Compression**
   - Optional snapshot compression
   - Reduces disk space, slight CPU overhead

### Benchmarks

```
Snapshot Save (1000 mobiles, 5000 items): 150ms
Journal Append (single operation): <1ms
Journal Replay (10000 entries): 500ms
Query (immutable snapshot): <10ms
```

## Entity Serialization

### Mobile Entity

```csharp
[MemoryPackable]
public partial struct UOMobileEntity
{
    public Serial Serial;
    public string Name;
    public int Body;
    public int Hue;
    public Point3D Position;
    public Map Facet;
    public Serial BackpackId;
    public List<Serial> EquippedItemIds;
    public Dictionary<SkillId, int> Skills;
}
```

### Item Entity

```csharp
[MemoryPackable]
public partial struct UOItemEntity
{
    public Serial Serial;
    public int ItemId;
    public int Amount;
    public int Hue;
    public Point3D Position;
    public Serial? ParentContainerId;
    public ContainerPosition PositionInContainer;
    public Serial? EquippedMobileId;
    public Layer EquippedLayer;
}
```

## Next Steps

- **[Data Format](format.md)** - Binary serialization details
- **[Repositories](repositories.md)** - Query and data access
- **[Scripting](../scripting/overview.md)** - Lua scripting

---

**Previous**: [Solution Structure](../architecture/solution.md) | **Next**: [Data Format](format.md)
