# Moongate v2

<p align="center">
  <img src="moongate_logo.png" alt="Moongate logo" width="240" />
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
- Session split between transport (`GameNetworkSession`) and gameplay/protocol context (`GameSession`).
- Unit tests for core server behaviors and packet infrastructure.

For a detailed internal status snapshot, see `docs/plans/status-2026-02-17.md`.

## Solution Structure

- `src/Moongate.Server`: host/bootstrap, game loop, network orchestration, session/event services.
- `src/Moongate.Network.Packets`: packet contracts, descriptors, registry, packet definitions.
- `src/Moongate.Network.Packets.Generators`: source generator for packet table registration.
- `src/Moongate.UO.Data`: UO domain data types and utility models.
- `src/Moongate.Core`: shared low-level utilities.
- `Moongate.Network`: TCP/network primitives.
- `tests/Moongate.Tests`: unit tests.
- `docs/`: Obsidian knowledge base (plans, sprints, protocol notes, journal).

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

## Docker

Build the image:

```bash
docker build -t moongatev2 .
```

Run the container:

```bash
docker run --rm -it -p 2593:2593 moongatev2
```

The Dockerfile publishes a NativeAOT binary and runs it on Alpine (`linux-musl` runtime).

## Documentation

Project documentation (Obsidian vault) is in `docs/`.

- Docs home: `docs/Home.md`
- Development plan: `docs/plans/moongate-v2-development-plan.md`
- Current status snapshot: `docs/plans/status-2026-02-17.md`
- Sprint tracking: `docs/sprints/sprint-001.md`
- Protocol notes index: `docs/protocol/README.md`

## Development Notes

- Shared build/analyzer/version settings are centralized in `Directory.Build.props`.
- Current global version baseline: `0.1.0`.
- CI currently validates build and tests; AOT publish gate is planned.

## License

This project is licensed under the GNU General Public License v3.0.
See `LICENSE` for details.
