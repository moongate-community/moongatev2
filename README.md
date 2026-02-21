# Moongate v2

<p align="center">
  <img src="images/moongate_logo.png" alt="Moongate logo" width="240" />
</p>

<p align="center">
  <img src="https://img.shields.io/badge/platform-.NET%2010-blueviolet" alt=".NET 10">
  <img src="https://img.shields.io/badge/AOT-enabled-green" alt="AOT Enabled">
  <img src="https://img.shields.io/badge/scripting-Lua-yellow" alt="Lua Scripting">
  <img src="https://img.shields.io/badge/license-GPL--3.0-blue" alt="GPL-3.0 License">
  <img src="https://img.shields.io/badge/status-development-orange" alt="Development Status">
</p>

[![CI](https://github.com/moongate-community/moongatev2/actions/workflows/ci.yml/badge.svg)](https://github.com/moongate-community/moongatev2/actions/workflows/ci.yml)
[![Release](https://github.com/moongate-community/moongatev2/actions/workflows/release.yml/badge.svg)](https://github.com/moongate-community/moongatev2/actions/workflows/release.yml)

Moongate v2 is a modern Ultima Online server project built with .NET 10.
It targets a clean, modular architecture with strong packet tooling, deterministic game-loop processing, and practical test coverage.

## Project Goals

- Build a maintainable UO server foundation focused on correctness and iteration speed.
- Keep networking and game-loop boundaries explicit and thread-safe.
- Model protocol packets with typed definitions and source-generated registration.
- Stay AOT-aware while preserving a smooth local development workflow.

## Current Status

The project is actively in development and already includes:

- TCP server startup and connection lifecycle handling.
- Packet framing/parsing for fixed and variable packet sizes.
- Attribute-based packet mapping (`[PacketHandler(...)]`) with source generation.
- Inbound message bus (`IMessageBusService`) for network thread -> game-loop crossing.
- Domain event bus (`IGameEventBusService`) with initial events (`PlayerConnectedEvent`, `PlayerDisconnectedEvent`).
- Outbound event listener abstraction (`IOutboundEventListener<TEvent>`) for domain-event -> network side effects.
- Session split between transport (`GameNetworkSession`) and gameplay/protocol context (`GameSession`).
- Unit tests for core server behaviors and packet infrastructure.
- Lua scripting runtime with module/function binding and `.luarc` generation support.
- Embedded HTTP host (`Moongate.Server.Http`) for health/admin endpoints and OpenAPI/Scalar docs.
- Dedicated HTTP rolling logs in the shared logs directory (`moongate_http-*.log`).
- Snapshot+journal persistence module (`Moongate.Persistence`) integrated in server lifecycle.
- ID-based persistence references for character equipment/container ownership.
- Interactive console UI with fixed prompt (`moongate>`) and Spectre-based colored log rendering.
- Timer wheel runtime metrics integrated in the metrics pipeline (`timer.*`).
- Timestamp-driven game loop scheduling with timer delta updates and optional idle CPU throttling.

For a detailed internal status snapshot, see `docs/plans/status-2026-02-19.md`.

## Persistence

Moongate uses a lightweight file-based persistence model implemented in `src/Moongate.Persistence`:

- Snapshot file (`world.snapshot.bin`) for full world state checkpoints.
- Append-only journal (`world.journal.bin`) for incremental operations between snapshots.
- MemoryPack binary serialization for compact and fast read/write.
- Per-operation checksums in journal entries to detect truncated/corrupted tails.
- Thread-safe repositories for accounts, mobiles, and items.
- Mobile/item relations are persisted by serial references:
  - `UOMobileEntity.BackpackId`
  - `UOMobileEntity.EquippedItemIds`
  - `UOItemEntity.ParentContainerId` + `ContainerPosition`
  - `UOItemEntity.EquippedMobileId` + `EquippedLayer`

Runtime behavior:

- On startup, `IPersistenceService.StartAsync()` loads snapshot (if present) and replays journal.
- During runtime, repositories append operations to journal.
- On save/stop, `SaveSnapshotAsync()` writes a new snapshot and resets the journal.

Storage location:

- Files are written under the server `save` directory (`DirectoriesConfig[DirectoryType.Save]`).

Query support:

- `IAccountRepository`, `IMobileRepository`, and `IItemRepository` expose `QueryAsync(...)`.
- Queries are evaluated on immutable snapshots with ZLinq-backed projection/filtering.

## Templates

Moongate loads gameplay templates from `DirectoriesConfig[DirectoryType.Templates]`:

- `templates/items/**/*.json` -> loaded by `ItemTemplateLoader` into `IItemTemplateService`
- `templates/mobiles/**/*.json` -> loaded by `MobileTemplateLoader` into `IMobileTemplateService`

Template values are data-driven and resolved at runtime using spec objects:

- `HueSpec`: supports fixed values (`"4375"`, `"0x1117"`) and ranges (`"hue(5:55)"`)
- `GoldValueSpec`: supports fixed values (`"0"`) and dice notation (`"dice(1d8+8)"`)

Example item template:

```json
{
  "type": "item",
  "id": "leather_backpack",
  "name": "Leather Backpack",
  "category": "Container",
  "itemId": "0x0E76",
  "hue": "hue(10:80)",
  "goldValue": "dice(2d8+12)",
  "lootType": "Regular",
  "stackable": false,
  "isMovable": true
}
```

Example startup item template:

```json
{
  "type": "item",
  "id": "inner_torso",
  "category": "Start Clothes",
  "itemId": "0x1F7B",
  "hue": "4375",
  "goldValue": "dice(1d4+1)",
  "weight": 1
}
```

Example mobile template:

```json
{
  "type": "mobile",
  "id": "orione",
  "name": "Orione",
  "category": "animals",
  "body": "0xC9",
  "skinHue": 779,
  "hairStyle": 0,
  "brain": "orion"
}
```

Resolution model:

- JSON loading parses to typed specs (`HueSpec`, `GoldValueSpec`)
- final random values are resolved when creating runtime entities (not at JSON load time)

## Solution Structure

- `src/Moongate.Server`: host/bootstrap, game loop, network orchestration, session/event services.
- `src/Moongate.Network.Packets`: packet contracts, descriptors, registry, packet definitions.
- `src/Moongate.Network.Packets.Generators`: source generator for packet table registration.
- `src/Moongate.UO.Data`: UO domain data types and utility models.
- `src/Moongate.Core`: shared low-level utilities.
- `src/Moongate.Network`: TCP/network primitives.
- `src/Moongate.Scripting`: Lua engine service, script modules, script loaders, and scripting helpers.
- `src/Moongate.Server.Http`: embedded ASP.NET Core host service used by the server bootstrap.
- `tests/Moongate.Tests`: unit tests.
- `docs/`: Obsidian knowledge base (plans, sprints, protocol notes, journal).

## Event And Packet Separation

Moongate uses a strict separation between inbound protocol parsing and outbound event projections:

- `IPacketListener` handles inbound packets only (`Client -> Server`) and applies domain use-cases.
- Domain services publish `IGameEvent` messages through `IGameEventBusService`.
- `IOutboundEventListener<TEvent>` handles outbound side-effects from domain events (for example enqueueing packets).
- `RegisterOutboundEventListener<TEvent, TListener>()` is the bootstrap helper to register outbound listeners as hosted services with priority.
- `IOutgoingPacketQueue` and `IOutboundPacketSender` deliver outbound packets on the game-loop/network boundary.

## Game Loop Scheduling

The server loop is timestamp-driven (monotonic `Stopwatch`) rather than fixed-sleep tick stepping:

- `GameLoopService` computes current loop timestamp and calls `ITimerService.UpdateTicksDelta(...)`.
- `TimerWheelService` accumulates elapsed milliseconds and advances only the required number of wheel ticks.
- This keeps timer semantics stable while adapting to real runtime load.
- Optional idle throttling (`Game.IdleCpuEnabled`, `Game.IdleSleepMilliseconds`) sleeps briefly when no work was processed.

## Requirements

- .NET SDK 10.0.x

## Quick Start

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/Moongate.Server
```

By default, the server starts with packet data logging enabled in `Program.cs`.

Console logging:

- Custom Serilog console sink with output template compatible formatting.
- Level-based colored output in terminal (Spectre.Console).
- Placeholder values (message properties) highlighted with dedicated styling.
- Fixed bottom prompt row (`moongate>`) when running in an interactive terminal.

HTTP service defaults:

- `Http.IsEnabled = true`
- `Http.Port = 8088`
- `Http.IsOpenApiEnabled = true`
- Base endpoint: `/`
- Health endpoint: `/health`
- OpenAPI JSON: `/openapi/v1.json`
- Scalar UI: `/scalar`

## Scripting

Moongate includes a Lua scripting subsystem in `src/Moongate.Scripting`, based on MoonSharp.

- `LuaScriptEngineService` handles script execution, callbacks, constants, and function invocation.
- Script modules are exposed with attributes (`[ScriptModule]`, `[ScriptFunction]`).
- `LuaScriptLoader` resolves scripts from configured script directories.
- `.luarc` metadata generation is included to improve editor tooling.

Current automated coverage includes:

- `LuaScriptLoader` file resolution and load behavior.
- `LuaScriptEngineService` constants, callbacks, module calls, error path, and naming conversions.
- `ScriptResultBuilder` success/error contract behavior.

Example script callback (for example in `<root>/scripts/init.lua`):

```lua
function on_player_connected(p)
	log.info("Anvedi che s'e connesson un client")
end
```

## Scripts

Repository helper scripts in `scripts/`:

- `scripts/build_image.sh`: builds the Docker image using `docker buildx`, with options for tag, platform, push, and no-cache.
- `scripts/run_aot.sh`: publishes and runs the server with NativeAOT settings for local AOT verification.

## Docker

Build the image:

```bash
./scripts/build_image.sh -t moongate-server:local
```

Run the container:

```bash
docker run --rm -it \
  -p 2593:2593 \
  -v /path/host/moongate-root:/app \
  -v /path/host/uo-client:/uo:ro \
  --name moongate \
  moongate-server:local
```

The Docker image publishes a NativeAOT binary and runs it on Alpine (`linux-musl` runtime).
Container defaults:

- `MOONGATE_ROOT_DIRECTORY=/app`
- `MOONGATE_UO_DIRECTORY=/uo`

`/path/host/uo-client` must contain required UO client files (e.g. `client.exe`).

Console behavior in Docker:

- Run with `-it` to enable the interactive prompt UI (`moongate>`).
- Without TTY (`-it` omitted), logs still work but prompt interaction is disabled.

## Documentation

Project documentation (Obsidian vault) is in `docs/`.

- Docs home: `docs/Home.md`
- Development plan: `docs/plans/moongate-v2-development-plan.md`
- Current status snapshot: `docs/plans/status-2026-02-19.md`
- Sprint tracking: `docs/sprints/sprint-001.md`
- Sprint closeout: `docs/sprints/sprint-001-closeout-2026-02-18.md`
- Protocol notes index: `docs/protocol/README.md`

## Development Notes

- Shared build/analyzer/version settings are centralized in `Directory.Build.props`.
- Current global version baseline: `0.2.0`.
- CI currently validates build and tests; AOT publish gate is planned.

## License

This project is licensed under the GNU General Public License v3.0.
See `LICENSE` for details.
