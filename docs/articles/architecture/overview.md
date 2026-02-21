# Architecture Overview

High-level overview of Moongate v2's system architecture.

## System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         Moongate v2 Server                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐               │
│  │   UO Client  │───▶│   TCP Server │───▶│   Packet     │               │
│  │   (Port 2593)│    │   (Network)  │    │   Handlers   │               │
│  └──────────────┘    └──────────────┘    └──────────────┘               │
│                                              │                           │
│                                              ▼                           │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐               │
│  │   HTTP       │◀───│   Game Loop  │◀───│   Message    │               │
│  │   Metrics    │    │   (Timer)    │    │   Bus        │               │
│  │   (Port 8088)│    │              │    │              │               │
│  └──────────────┘    └──────────────┘    └──────────────┘               │
│                           │                                              │
│                           ▼                                              │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐               │
│  │   Lua        │◀───│   Event      │───▶│   Persistence│               │
│  │   Scripting  │    │   Bus        │    │   (Snapshot) │               │
│  └──────────────┘    └──────────────┘    └──────────────┘               │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

## Core Design Principles

### 1. Separation of Concerns

Moongate v2 enforces strict boundaries between:

- **Network Layer**: TCP connections, packet framing, protocol parsing
- **Game Layer**: Game logic, entity management, world state
- **Persistence Layer**: Data storage, snapshots, journals
- **Scripting Layer**: Lua runtime, custom gameplay logic

### 2. Thread Safety

- **Network Thread**: Handles incoming packets, pushes to message bus
- **Game Loop Thread**: Processes messages, updates world state
- **No Shared Mutable State**: Communication via message passing

### 3. Event-Driven Architecture

- **Inbound Packets** → `IPacketListener` → Domain Logic
- **Domain Events** → `IGameEventBusService` → `IOutboundEventListener`
- **Outbound Packets** → `IOutgoingPacketQueue` → Network

### 4. AOT-First Design

- NativeAOT compilation from day one
- No reflection in hot paths
- Source generators for packet registration
- Predictable performance characteristics

## Solution Structure

```
moongatev2/
├── src/
│   ├── Moongate.Abstractions/        # Core interfaces and contracts
│   ├── Moongate.Core/                # Shared utilities and types
│   ├── Moongate.Network/             # TCP server, connections, buffers
│   ├── Moongate.Network.Packets/     # Packet definitions and handlers
│   ├── Moongate.Network.Packets.Generators/  # Source generators
│   ├── Moongate.Server/              # Main server bootstrap and composition
│   ├── Moongate.Server.Http/         # Embedded HTTP server
│   ├── Moongate.Server.Metrics/      # Metrics collection
│   ├── Moongate.Server.Metrics.Generators/   # Metrics source generators
│   ├── Moongate.Persistence/         # Snapshot + Journal persistence
│   ├── Moongate.Scripting/           # Lua scripting engine
│   └── Moongate.UO.Data/             # UO domain types and utilities
├── tests/
│   └── Moongate.Tests/               # Unit tests
└── docs/                             # Documentation
```

## Component Responsibilities

### Moongate.Abstractions

Core interfaces and contracts used across all services:

- `IGameEvent` - Domain event base interface
- `IPacketHandler` - Packet handler contract
- `ISession` - Session abstraction
- `IRepository<T>` - Data repository pattern

### Moongate.Core

Shared utilities and foundational types:

- `Serial` - Unique identifier for game objects
- `Point2D`, `Point3D` - Coordinate types
- `ClientVersion` - UO client version handling
- JSON converters and serialization

### Moongate.Network

TCP server and network primitives:

- `GameTcpServer` - TCP listener and connection management
- `GameNetworkSession` - Per-client network session
- `SpanReader`, `SpanWriter` - Zero-allocation packet I/O
- `NetworkBufferPool` - Buffer recycling

### Moongate.Network.Packets

Packet definitions and handling:

- `[PacketHandler(0x01, "Packet Name")]` - Attribute-based registration
- `IPacketListener` - Inbound packet processing
- `PacketRegistry` - Source-generated packet table
- Incoming/Outgoing packet types

### Moongate.Server

Main server composition root:

- `MoongateBootstrap` - Application startup
- `GameLoopService` - Timestamp-driven game loop
- `TimerWheelService` - Efficient timer scheduling
- Service registration and DI setup

### Moongate.Server.Http

Embedded HTTP server:

- ASP.NET Core Kestrel hosting
- `/health` - Health check endpoint
- `/metrics` - Prometheus metrics
- `/scalar` - OpenAPI documentation

### Moongate.Persistence

File-based persistence:

- `IPersistenceService` - Snapshot and journal management
- `IRepository<T>` - Thread-safe data access
- MemoryPack serialization
- Checksum validation for data integrity

### Moongate.Scripting

Lua scripting engine:

- `LuaScriptEngineService` - Script execution
- `[ScriptModule]`, `[ScriptFunction]` - .NET → Lua exposure
- `.luarc` generation for editor tooling
- Callback system for game events

## Data Flow

### Inbound Packet Flow

```
UO Client
    │
    ▼
TCP Socket
    │
    ▼
GameNetworkSession (Parse packet)
    │
    ▼
IPacketListener (Handle packet)
    │
    ▼
IMessageBusService (Enqueue message)
    │
    ▼
Game Loop (Process message)
    │
    ▼
Domain Logic (Apply changes)
```

### Outbound Packet Flow

```
Domain Event (e.g., PlayerConnectedEvent)
    │
    ▼
IGameEventBusService (Publish event)
    │
    ▼
IOutboundEventListener (Handle event)
    │
    ▼
IOutgoingPacketQueue (Enqueue packet)
    │
    ▼
GameNetworkSession (Send packet)
    │
    ▼
UO Client
```

## Key Architectural Patterns

### Message Bus Pattern

Decouples network thread from game loop:

```csharp
// Network thread enqueues
messageBus.Enqueue(packet, session);

// Game loop processes
while (messageBus.TryDequeue(out var message))
{
    ProcessMessage(message);
}
```

### Event Bus Pattern

Decouples domain logic from side effects:

```csharp
// Domain service publishes
eventBus.Publish(new PlayerConnectedEvent(player));

// Listener handles
[OutboundEventListener(typeof(PlayerConnectedEvent))]
public class PlayerConnectedListener : IOutboundEventListener<PlayerConnectedEvent>
{
    public void Handle(PlayerConnectedEvent evt)
    {
        // Send welcome packet
    }
}
```

### Repository Pattern

Abstracts data access:

```csharp
// Query accounts
var accounts = await accountRepository.QueryAsync(q =>
    q.Where(a => a.Username == username)
     .Select(a => a.Id)
);

// Save account
await accountRepository.SaveAsync(account);
```

### Timer Wheel Pattern

Efficient timer scheduling:

```csharp
// Schedule timer
var timerId = timerService.Schedule(
    interval: TimeSpan.FromSeconds(30),
    callback: () => SaveDatabase()
);

// Timer wheel advances based on elapsed time
timerService.UpdateTicksDelta(elapsedMilliseconds);
```

## Performance Optimizations

### Zero-Allocation Packet Parsing

Uses `Span<T>` for stack-based parsing:

```csharp
var reader = new SpanReader(packetSpan);
var serial = reader.ReadUInt32();  // No heap allocation
```

### Buffer Pooling

Recycles network buffers:

```csharp
var buffer = bufferPool.Rent();
try
{
    // Use buffer
}
finally
{
    bufferPool.Return(buffer);
}
```

### Source Generators

Compile-time packet registration:

```csharp
// Source generator creates packet table at compile time
[PacketHandler(0x01, "Disconnect")]
public sealed class DisconnectHandler : IPacketHandler
{
    // ...
}
```

### NativeAOT

Ahead-of-Time compilation benefits:

- **Faster Startup**: No JIT compilation
- **Lower Memory**: No JIT compiler in memory
- **Predictable**: No runtime compilation pauses
- **Single Binary**: Easy deployment

## Next Steps

- **[Network System](network.md)** - Deep dive into networking
- **[Game Loop](game-loop.md)** - Timestamp-driven scheduling
- **[Event System](events.md)** - Domain events and message bus
- **[Session Management](sessions.md)** - Client session handling

---

**Previous**: [Configuration](../getting-started/configuration.md) | **Next**: [Network System](network.md)
