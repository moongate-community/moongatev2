# Data Repositories

Repository pattern for data access in Moongate v2.

## Overview

Moongate v2 uses the repository pattern to provide:

- Thread-safe data access
- Immutable snapshots for queries
- ZLinq-backed LINQ queries
- Change tracking for journaling

## Repository Interfaces

### IAccountRepository

```csharp
public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Serial serial, CancellationToken cancellationToken);
    Task<Account?> GetByUsernameAsync(string username, CancellationToken cancellationToken);
    Task SaveAsync(Account account, CancellationToken cancellationToken);
    Task DeleteAsync(Serial serial, CancellationToken cancellationToken);
    IQueryResult<Account> QueryAsync(Func<IQueryable<Account>, IQueryable<Account>> query);
    IReadOnlyList<Account> GetAll();
}
```

### IMobileRepository

```csharp
public interface IMobileRepository
{
    Task<Mobile?> GetByIdAsync(Serial serial, CancellationToken cancellationToken);
    Task<Mobile?> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task SaveAsync(Mobile mobile, CancellationToken cancellationToken);
    Task DeleteAsync(Serial serial, CancellationToken cancellationToken);
    IQueryResult<Mobile> QueryAsync(Func<IQueryable<Mobile>, IQueryable<Mobile>> query);
    IReadOnlyList<Mobile> GetAll();
    IReadOnlyList<Mobile> GetInRegion(string region, Map map);
}
```

### IItemRepository

```csharp
public interface IItemRepository
{
    Task<Item?> GetByIdAsync(Serial serial, CancellationToken cancellationToken);
    Task SaveAsync(Item item, CancellationToken cancellationToken);
    Task DeleteAsync(Serial serial, CancellationToken cancellationToken);
    IQueryResult<Item> QueryAsync(Func<IQueryable<Item>, IQueryable<Item>> query);
    IReadOnlyList<Item> GetAll();
    IReadOnlyList<Item> GetInContainer(Serial containerSerial);
}
```

## Query Examples

### Account Queries

```csharp
// Get accounts created in last 7 days
var recentAccounts = await _accountRepository.QueryAsync(q =>
    q.Where(a => a.CreatedAt > DateTime.UtcNow.AddDays(-7))
     .OrderByDescending(a => a.CreatedAt)
     .Take(10)
     .ToList()
);

// Get accounts by login activity
var activeAccounts = await _accountRepository.QueryAsync(q =>
    q.Where(a => a.LastLogin > DateTime.UtcNow.AddDays(-1))
     .Select(a => new { a.Username, a.LastLogin })
     .OrderByDescending(a => a.LastLogin)
     .ToList()
);

// Get account count
var accountCount = await _accountRepository.QueryAsync(q =>
    q.Count()
);
```

### Mobile Queries

```csharp
// Get mobiles in specific area
var mobilesInArea = await _mobileRepository.QueryAsync(q =>
    q.Where(m => m.Position.X >= 1000 && m.Position.X <= 2000)
     .Where(m => m.Position.Y >= 2000 && m.Position.Y <= 3000)
     .Where(m => m.Position.Map == Map.Britannia)
     .ToList()
);

// Get NPCs (mobiles without owner)
var npcs = await _mobileRepository.QueryAsync(q =>
    q.Where(m => m.OwnerAccount == null)
     .Where(m => !m.IsPlayer())
     .ToList()
);

// Get mobiles by body type
var humans = await _mobileRepository.QueryAsync(q =>
    q.Where(m => m.Body is >= 0x0190 and <= 0x0193)
     .ToList()
);
```

### Item Queries

```csharp
// Get items in world (not in containers)
var worldItems = await _itemRepository.QueryAsync(q =>
    q.Where(i => i.ParentContainerId == null)
     .Where(i => i.EquippedMobileId == null)
     .ToList()
);

// Get stacked items
var stackedItems = await _itemRepository.QueryAsync(q =>
    q.Where(i => i.Amount > 1)
     .OrderByDescending(i => i.Amount)
     .Take(100)
     .ToList()
);

// Get containers with contents
var containers = await _itemRepository.QueryAsync(q =>
    q.Where(i => i.IsContainer)
     .Where(i => i.Contents.Count > 0)
     .Select(i => new { i.Serial, i.Name, ContentCount = i.Contents.Count })
     .ToList()
);
```

## Implementation

### Base Repository

```csharp
public abstract class BaseRepository<TEntity>
{
    protected readonly ISnapshotManager _snapshotManager;
    protected readonly IJournalService _journalService;
    protected readonly ILogger _logger;
    
    protected BaseRepository(
        ISnapshotManager snapshotManager,
        IJournalService journalService,
        ILogger logger)
    {
        _snapshotManager = snapshotManager;
        _journalService = journalService;
        _logger = logger;
    }
    
    public IQueryResult<TEntity> QueryAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        // Get immutable snapshot
        var snapshot = _snapshotManager.GetCurrentSnapshot<TEntity>();
        
        // Execute query on snapshot
        var result = query(snapshot.AsQueryable()).ToList();
        
        return new QueryResult<TEntity>(result);
    }
    
    public IReadOnlyList<TEntity> GetAll()
    {
        return _snapshotManager.GetCurrentSnapshot<TEntity>().ToList();
    }
}
```

### Account Repository

```csharp
public sealed class AccountRepository : BaseRepository<Account>, IAccountRepository
{
    public AccountRepository(
        ISnapshotManager snapshotManager,
        IJournalService journalService,
        ILogger<AccountRepository> logger)
        : base(snapshotManager, journalService, logger)
    {
    }
    
    public async Task<Account?> GetByIdAsync(Serial serial, CancellationToken cancellationToken)
    {
        var accounts = GetAll();
        return accounts.FirstOrDefault(a => a.Serial == serial);
    }
    
    public async Task<Account?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        var accounts = GetAll();
        return accounts.FirstOrDefault(a => a.Username == username);
    }
    
    public async Task SaveAsync(Account account, CancellationToken cancellationToken)
    {
        // Add to snapshot
        _snapshotManager.Update(account);
        
        // Append to journal
        await _journalService.AppendAsync(new JournalEntry
        {
            Operation = JournalOperationType.AccountUpdated,
            Data = MemoryPackSerializer.Serialize(account)
        }, cancellationToken);
        
        _logger.LogDebug("Saved account {Username}", account.Username);
    }
    
    public async Task DeleteAsync(Serial serial, CancellationToken cancellationToken)
    {
        // Remove from snapshot
        _snapshotManager.Remove<Account>(serial);
        
        // Append to journal
        await _journalService.AppendAsync(new JournalEntry
        {
            Operation = JournalOperationType.AccountDeleted,
            Data = MemoryPackSerializer.Serialize(new { Serial = serial })
        }, cancellationToken);
        
        _logger.LogDebug("Deleted account {Serial}", serial);
    }
}
```

## Thread Safety

### Immutable Snapshots

```csharp
public sealed class SnapshotManager
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<Type, object> _snapshots = new();
    
    public IReadOnlyList<TEntity> GetCurrentSnapshot<TEntity>()
    {
        _lock.EnterReadLock();
        try
        {
            if (_snapshots.TryGetValue(typeof(TEntity), out var snapshot))
            {
                return (IReadOnlyList<TEntity>)snapshot;
            }
            return Array.Empty<TEntity>();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
    
    public void Update<TEntity>(TEntity entity) where TEntity : IEntity
    {
        _lock.EnterWriteLock();
        try
        {
            var snapshot = GetCurrentSnapshot<TEntity>().ToList();
            var index = snapshot.FindIndex(e => e.Serial == entity.Serial);
            
            if (index >= 0)
            {
                snapshot[index] = entity;
            }
            else
            {
                snapshot.Add(entity);
            }
            
            _snapshots[typeof(TEntity)] = snapshot.AsReadOnly();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}
```

## Change Tracking

### Unit of Work Pattern

```csharp
public interface IUnitOfWork : IDisposable
{
    IAccountRepository Accounts { get; }
    IMobileRepository Mobiles { get; }
    IItemRepository Items { get; }
    Task CommitAsync(CancellationToken cancellationToken);
    Task RollbackAsync();
}

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IAccountRepository _accounts;
    private readonly IMobileRepository _mobiles;
    private readonly IItemRepository _items;
    private readonly IJournalService _journal;
    private readonly List<JournalEntry> _pendingChanges = new();
    
    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        // Append all changes to journal
        foreach (var entry in _pendingChanges)
        {
            await _journal.AppendAsync(entry, cancellationToken);
        }
        
        _pendingChanges.Clear();
    }
    
    public async Task RollbackAsync()
    {
        _pendingChanges.Clear();
        await Task.CompletedTask;
    }
    
    public void Dispose()
    {
        RollbackAsync().GetAwaiter().GetResult();
    }
}
```

## Usage Example

### Complete Example

```csharp
public sealed class CharacterCreationService
{
    private readonly IAccountRepository _accounts;
    private readonly IMobileRepository _mobiles;
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Serial> CreateCharacterAsync(
        Serial accountSerial,
        string name,
        int body,
        int hue,
        Point3D location,
        CancellationToken cancellationToken)
    {
        // Validate name uniqueness
        var existing = await _mobiles.GetByNameAsync(name, cancellationToken);
        if (existing != null)
        {
            throw new InvalidOperationException("Name already exists");
        }
        
        // Create mobile
        var mobile = new Mobile
        {
            Serial = Serial.GenerateMobile(),
            Name = name,
            Body = body,
            Hue = hue,
            Position = location,
            OwnerAccount = accountSerial
        };
        
        // Save mobile
        await _mobiles.SaveAsync(mobile, cancellationToken);
        
        // Update account
        var account = await _accounts.GetByIdAsync(accountSerial, cancellationToken);
        account.Characters.Add(mobile.Serial);
        await _accounts.SaveAsync(account, cancellationToken);
        
        // Commit changes
        await _unitOfWork.CommitAsync(cancellationToken);
        
        return mobile.Serial;
    }
}
```

## Performance

### Query Optimization

```csharp
// GOOD: Filter early, select late
var result = await _mobileRepository.QueryAsync(q =>
    q.Where(m => m.Position.Map == Map.Britannia)
     .Where(m => m.Position.X > 1000 && m.Position.X < 2000)
     .Select(m => new { m.Serial, m.Name })
     .ToList()
);

// BAD: Select early, filter late
var result = await _mobileRepository.QueryAsync(q =>
    q.Select(m => new { m.Serial, m.Name })
     .Where(a => /* can't filter by position */)
     .ToList()
);
```

### Batch Operations

```csharp
// GOOD: Batch save
public async Task SaveBatchAsync(IEnumerable<Mobile> mobiles, CancellationToken cancellationToken)
{
    foreach (var mobile in mobiles)
    {
        _snapshotManager.Update(mobile);
        _pendingChanges.Add(new JournalEntry
        {
            Operation = JournalOperationType.MobileUpdated,
            Data = MemoryPackSerializer.Serialize(mobile)
        });
    }
    
    // Single journal flush
    await _journalService.AppendBatchAsync(_pendingChanges, cancellationToken);
}

// BAD: Individual saves
public async Task SaveIndividuallyAsync(IEnumerable<Mobile> mobiles, CancellationToken cancellationToken)
{
    foreach (var mobile in mobiles)
    {
        await _mobiles.SaveAsync(mobile, cancellationToken);  // Multiple flushes
    }
}
```

## Next Steps

- **[Data Format](format.md)** - Binary serialization details
- **[Persistence Overview](overview.md)** - System architecture
- **[Scripting API](../scripting/api.md)** - Scripting data access

---

**Previous**: [Data Format](format.md) | **Next**: [Protocol Reference](../networking/protocol.md)
