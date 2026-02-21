# Introduction to Moongate v2

## What is Moongate v2?

**Moongate v2** is a modern, high-performance Ultima Online server emulator built from the ground up with **.NET 10** and **NativeAOT** (Ahead-of-Time) compilation. It represents a complete rewrite of the original Moongate project, focusing on clean architecture, explicit boundaries, and practical performance.

## Project Vision

Moongate v2 is not a clone of ModernUO, RunUO, ServUO, or any other emulator. While we owe inspiration to these projects and their invaluable contributions to the UO community, Moongate v2 follows its own path:

### Core Principles

1. **Performance First** - Leveraging .NET 10 AOT for maximum speed and predictable performance
2. **Explicit Architecture** - Clear boundaries between networking, game logic, and persistence
3. **Thread Safety** - Deterministic game-loop processing with safe cross-thread communication
4. **Modern Tooling** - Source generators, typed packet definitions, OpenAPI documentation
5. **Accessible Scripting** - Lua-based customization for server administrators

## Key Technologies

| Technology | Purpose |
|------------|---------|
| **.NET 10** | Latest .NET runtime with performance improvements |
| **NativeAOT** | Ahead-of-Time compilation for faster startup, lower memory |
| **MoonSharp** | Lua scripting engine for gameplay customization |
| **Serilog** | Structured logging with console and file sinks |
| **Spectre.Console** | Rich terminal UI with colored output |
| **MemoryPack** | High-performance binary serialization for persistence |
| **ZLinq** | LINQ-like queries for data repositories |

## Architecture Highlights

### Network Layer

- Custom TCP server optimized for UO protocol
- Packet framing and parsing for fixed/variable sizes
- Source-generated packet registration via `[PacketHandler]` attributes
- Inbound message bus for network → game-loop communication

### Game Loop

- Timestamp-driven scheduling (monotonic `Stopwatch`)
- Timer wheel for efficient event scheduling
- Optional idle CPU throttling
- Deterministic tick processing

### Event System

- Strict separation: inbound packets vs outbound events
- `IPacketListener` for client → server handling
- `IGameEventBusService` for domain event publishing
- `IOutboundEventListener<TEvent>` for event → network side effects

### Persistence

- Snapshot file (`world.snapshot.bin`) for full state checkpoints
- Append-only journal (`world.journal.bin`) for incremental changes
- MemoryPack binary serialization
- Thread-safe repositories with ZLinq queries

### Scripting

- MoonSharp Lua runtime
- Attribute-based script modules (`[ScriptModule]`, `[ScriptFunction]`)
- Automatic `.luarc` generation for editor tooling
- Callback system for game events

## Performance Characteristics

NativeAOT compilation provides:

- **Faster Startup** - No JIT compilation at runtime
- **Lower Memory** - Reduced footprint without JIT compiler
- **Predictable Performance** - No runtime compilation pauses
- **Single Binary** - Easy deployment, especially in containers

## Current Status

Moongate v2 is **actively in development**. The following features are implemented:

### Implemented

- [x] TCP server startup and connection lifecycle
- [x] Packet framing/parsing (fixed and variable sizes)
- [x] Attribute-based packet mapping with source generation
- [x] Inbound message bus (`IMessageBusService`)
- [x] Domain event bus with `PlayerConnectedEvent`, `PlayerDisconnectedEvent`
- [x] Outbound event listeners
- [x] Session split (transport vs gameplay context)
- [x] Lua scripting runtime
- [x] Embedded HTTP server with OpenAPI/Scalar
- [x] Snapshot + Journal persistence
- [x] Interactive console UI
- [x] Timer wheel metrics
- [x] Unit tests for core systems

### Planned

- [ ] Character creation flow
- [ ] Full movement system
- [ ] Combat mechanics
- [ ] Skill system
- [ ] NPC AI
- [ ] Item system completion
- [ ] House/shelter system
- [ ] Guild system

## Who Is This For?

Moongate v2 is designed for:

- **Server Administrators** who want a performant, customizable UO server
- **Developers** interested in learning MMO server architecture
- **Contributors** who want to help build a modern UO emulator
- **Players** who want to run their own shards with unique features

## Getting Started

See the [Quick Start](quickstart.md) guide to get Moongate v2 running in minutes.

## Community & Support

- **GitHub**: https://github.com/moongate-community/moongatev2
- **Discord**: https://discord.gg/3HT7v95b
- **Docker Hub**: https://hub.docker.com/r/tgiachi/moongate

## License

Moongate v2 is licensed under the **GNU General Public License v3.0**.

---

**Next**: [Quick Start Guide](quickstart.md) - Get up and running in 5 minutes
