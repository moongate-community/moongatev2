# Solution Structure

Overview of Moongate v2's project organization and module responsibilities.

## Solution Tree

```
moongatev2/
├── src/
│   ├── Moongate.Abstractions/        # Core interfaces and contracts
│   ├── Moongate.Core/                # Shared utilities and types
│   ├── Moongate.Network/             # TCP server and connections
│   ├── Moongate.Network.Packets/     # Packet definitions
│   ├── Moongate.Network.Packets.Generators/  # Source generators
│   ├── Moongate.Server/              # Main server bootstrap
│   ├── Moongate.Server.Http/         # Embedded HTTP server
│   ├── Moongate.Server.Metrics/      # Metrics collection
│   ├── Moongate.Server.Metrics.Generators/   # Metrics generators
│   ├── Moongate.Persistence/         # Data persistence
│   ├── Moongate.Scripting/           # Lua scripting
│   └── Moongate.UO.Data/             # UO domain types
├── tests/
│   └── Moongate.Tests/               # Unit tests
├── docs/                             # Documentation (this folder)
├── stack/                            # Docker Compose monitoring
├── scripts/                          # Build scripts
└── Directory.Build.props             # Shared build settings
```

## Project Dependencies

```
Moongate.Server
├── Moongate.Network
│   ├── Moongate.Network.Packets
│   │   ├── Moongate.Abstractions
│   │   └── Moongate.Core
│   └── Moongate.Core
├── Moongate.Server.Http
├── Moongate.Server.Metrics
│   └── Moongate.Core
├── Moongate.Persistence
│   ├── Moongate.Abstractions
│   └── Moongate.Core
├── Moongate.Scripting
│   ├── Moongate.Abstractions
│   └── Moongate.Core
├── Moongate.UO.Data
│   └── Moongate.Core
└── Moongate.Core

Moongate.Abstractions
└── Moongate.Core
```

## Project Details

### Moongate.Abstractions

**Purpose**: Core interfaces and contracts used across all services.

**Key Types**:
- `IGameEvent` - Domain event base interface
- `IPacketHandler` - Packet handler contract
- `ISession` - Session abstraction
- `IRepository<T>` - Data repository pattern
- `IService` - Base service interface

**Dependencies**: Moongate.Core

**Example**:
```csharp
// src/Moongate.Abstractions/Events/IGameEvent.cs
public interface IGameEvent
{
    DateTime Timestamp { get; }
}

// src/Moongate.Abstractions/Networking/IPacketHandler.cs
public interface IPacketHandler
{
    int PacketId { get; }
    int Length { get; }
    string Name { get; }
    void Handle(SpanReader reader, GameNetworkSession session);
}
```

### Moongate.Core

**Purpose**: Shared utilities and foundational types.

**Key Types**:
- `Serial` - Unique identifier for game objects
- `Point2D`, `Point3D` - Coordinate types
- `ClientVersion` - UO client version handling
- `PlatformUtils` - Platform detection utilities
- JSON converters and serialization helpers

**Dependencies**: None (base library)

**Example**:
```csharp
// src/Moongate.Core/Types/Serial.cs
public readonly struct Serial : IEquatable<Serial>
{
    private readonly uint _value;
    
    public bool IsMobile => (_value & 0xC0000000) == 0;
    public bool IsItem => (_value & 0xC0000000) != 0;
    
    public static implicit operator uint(Serial serial) => serial._value;
    public static explicit operator Serial(uint value) => new(value);
}

// src/Moongate.Core/Types/Point3D.cs
public readonly struct Point3D
{
    public int X { get; }
    public int Y { get; }
    public int Z { get; }
}
```

### Moongate.Network

**Purpose**: TCP server, connections, and network primitives.

**Key Types**:
- `GameTcpServer` - TCP listener
- `GameNetworkSession` - Per-client session
- `SpanReader`, `SpanWriter` - Zero-allocation I/O
- `NetworkBufferPool` - Buffer recycling
- `INetworkService` - Network service contract

**Dependencies**: Moongate.Abstractions, Moongate.Core

**Example**:
```csharp
// src/Moongate.Network/Server/GameTcpServer.cs
public sealed class GameTcpServer
{
    private readonly TcpListener _listener;
    private readonly int _port;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _listener.Start();
        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync(cancellationToken);
            // Create session...
        }
    }
}
```

### Moongate.Network.Packets

**Purpose**: Packet definitions, handlers, and registry.

**Key Types**:
- `[PacketHandler]` attribute
- `IPacketListener` - Inbound packet processor
- `IOutgoingPacket` - Outgoing packet contract
- `PacketRegistry` - Source-generated packet table

**Dependencies**: Moongate.Abstractions, Moongate.Core, Moongate.Network

**Example**:
```csharp
// src/Moongate.Network.Packets/Attributes/PacketHandlerAttribute.cs
[AttributeUsage(AttributeTargets.Class)]
public sealed class PacketHandlerAttribute : Attribute
{
    public int PacketId { get; }
    public int Length { get; }
    public string Name { get; }
    
    public PacketHandlerAttribute(int packetId, string name, int length = -1)
    {
        PacketId = packetId;
        Name = name;
        Length = length;
    }
}
```

### Moongate.Network.Packets.Generators

**Purpose**: Source generators for packet registration.

**Key Features**:
- Scans for `[PacketHandler]` attributes
- Generates `PacketRegistry` class at compile time
- Eliminates runtime reflection

**Dependencies**: None (analyzer/generator)

**Example** (Generated Code):
```csharp
// Generated by source generator
public static partial class PacketRegistry
{
    public static readonly IPacketHandler[] Handlers = new IPacketHandler[256]
    {
        [0x02] = new MoveRequestHandler(),
        [0x06] = new DoubleClickHandler(),
        [0x80] = new LoginRequestHandler(),
        // ...
    };
}
```

### Moongate.Server

**Purpose**: Main server bootstrap and composition root.

**Key Types**:
- `MoongateBootstrap` - Application startup
- `GameLoopService` - Game loop
- `TimerWheelService` - Timer scheduling
- `Program.cs` - Entry point

**Dependencies**: All server projects

**Example**:
```csharp
// src/Moongate.Server/Bootstrap/MoongateBootstrap.cs
public sealed class MoongateBootstrap
{
    private readonly IHost _host;
    
    public MoongateBootstrap(BootstrapOptions options)
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services => ConfigureServices(services, options))
            .Build();
    }
    
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await _host.StartAsync(cancellationToken);
        await _host.WaitForShutdownAsync(cancellationToken);
    }
}
```

### Moongate.Server.Http

**Purpose**: Embedded HTTP server for metrics and admin.

**Key Features**:
- ASP.NET Core Kestrel hosting
- `/health` - Health check endpoint
- `/metrics` - Prometheus metrics
- `/scalar` - OpenAPI documentation

**Dependencies**: Moongate.Core

**Example**:
```csharp
// src/Moongate.Server.Http/HttpServerService.cs
public sealed class HttpServerService : BackgroundService
{
    private readonly IHost _httpHost;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _httpHost.StartAsync(stoppingToken);
        await _httpHost.WaitForShutdownAsync(stoppingToken);
    }
}
```

### Moongate.Server.Metrics

**Purpose**: Metrics collection and exposure.

**Key Types**:
- `MetricsSnapshot` - Metrics data container
- `MetricType` - Metric type enumeration
- `MetricSample` - Individual metric sample

**Dependencies**: Moongate.Core

**Example**:
```csharp
// src/Moongate.Server.Metrics/MetricsSnapshot.cs
public sealed class MetricsSnapshot
{
    public long CollectedAtUnixMs { get; init; }
    public IReadOnlyList<MetricSample> Samples { get; init; }
    
    public string ToPrometheusFormat()
    {
        var builder = new StringBuilder();
        builder.AppendLine("# generated by moongate");
        builder.AppendLine($"# collected_at_unix_ms {CollectedAtUnixMs}");
        
        foreach (var sample in Samples)
        {
            builder.AppendLine(sample.ToPrometheusLine());
        }
        
        return builder.ToString();
    }
}
```

### Moongate.Server.Metrics.Generators

**Purpose**: Source generators for metrics.

**Key Features**:
- `[Metric]` attribute for auto-collection
- Generates metric collection code
- Reduces boilerplate

### Moongate.Persistence

**Purpose**: File-based data persistence.

**Key Types**:
- `IPersistenceService` - Snapshot and journal management
- `IRepository<T>` - Data access
- `MemoryPack` serialization
- Checksum validation

**Dependencies**: Moongate.Abstractions, Moongate.Core

**Example**:
```csharp
// src/Moongate.Persistence/Services/PersistenceService.cs
public sealed class PersistenceService : IPersistenceService
{
    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        if (File.Exists(_snapshotPath))
        {
            await LoadSnapshotAsync();
        }
        await ReplayJournalAsync();
    }
    
    public async Task SaveSnapshotAsync(CancellationToken cancellationToken)
    {
        using var stream = File.Create(_snapshotPath);
        await MemoryPackSerializer.SerializeAsync(stream, _worldState);
    }
}
```

### Moongate.Scripting

**Purpose**: Lua scripting engine.

**Key Types**:
- `LuaScriptEngineService` - Script execution
- `[ScriptModule]`, `[ScriptFunction]` attributes
- `LuaScriptLoader` - Script file loading
- `.luarc` generation

**Dependencies**: Moongate.Abstractions, Moongate.Core

**Example**:
```csharp
// src/Moongate.Scripting/Attributes/ScriptModuleAttribute.cs
[AttributeUsage(AttributeTargets.Class)]
public sealed class ScriptModuleAttribute : Attribute
{
    public string Name { get; }
    
    public ScriptModuleAttribute(string name)
    {
        Name = name;
    }
}

// Usage
[ScriptModule("log")]
public sealed class LogModule
{
    [ScriptFunction("info")]
    public void Info(string message)
    {
        logger.LogInformation(message);
    }
}
```

### Moongate.UO.Data

**Purpose**: Ultima Online domain types and utilities.

**Key Types**:
- `Mobile`, `Item` - Game entity types
- `Map`, `Tile` - Map data types
- `Hue`, `ItemID` - UO-specific types
- JSON converters for UO types

**Dependencies**: Moongate.Core

**Example**:
```csharp
// src/Moongate.UO.Data/Entities/UOMobile.cs
public sealed class UOMobile
{
    public Serial Serial { get; set; }
    public string Name { get; set; }
    public int Body { get; set; }
    public int Hue { get; set; }
    public Point3D Position { get; set; }
    public Map Facet { get; set; }
    public Serial BackpackId { get; set; }
    public List<Serial> EquippedItemIds { get; set; }
}
```

### Moongate.Tests

**Purpose**: Unit tests for all projects.

**Test Categories**:
- Network tests
- Packet tests
- Game loop tests
- Persistence tests
- Scripting tests

**Dependencies**: All server projects (for testing)

**Example**:
```csharp
// tests/Moongate.Tests/Network/SpanReaderTests.cs
public class SpanReaderTests
{
    [Fact]
    public void ReadUInt32_CorrectValue()
    {
        var data = new byte[] { 0x01, 0x00, 0x00, 0x00 };
        var reader = new SpanReader(data);
        var value = reader.ReadUInt32();
        Assert.Equal(1u, value);
    }
}
```

## Build Order

1. **Moongate.Core** - Base library (no dependencies)
2. **Moongate.Abstractions** - Depends on Core
3. **Moongate.Network** - Depends on Abstractions, Core
4. **Moongate.Network.Packets** - Depends on Network
5. **Moongate.Network.Packets.Generators** - Independent (analyzer)
6. **Moongate.UO.Data** - Depends on Core
7. **Moongate.Persistence** - Depends on Abstractions, Core
8. **Moongate.Scripting** - Depends on Abstractions, Core
9. **Moongate.Server.Http** - Depends on Core
10. **Moongate.Server.Metrics** - Depends on Core
11. **Moongate.Server.Metrics.Generators** - Independent (analyzer)
12. **Moongate.Server** - Depends on all above

## Versioning

All projects share the same version via `Directory.Build.props`:

```xml
<Project>
  <PropertyGroup>
    <Version>0.7.10</Version>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

## Next Steps

- **[Scripting Overview](../scripting/overview.md)** - Lua scripting
- **[Persistence Overview](../persistence/overview.md)** - Data storage
- **[Networking](../networking/packets.md)** - Packet system

---

**Previous**: [Session Management](sessions.md) | **Next**: [Scripting Overview](../scripting/overview.md)
