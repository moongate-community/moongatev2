# Sprint 001 - Closeout (2026-02-18)

Related notes:
- [[sprint-001|Sprint 001 - M0 Skeleton]]
- [[sprint-001-kanban|Sprint 001 - Kanban]]
- [[../plans/status-2026-02-18|Current Status (2026-02-18)]]

## Sprint Objective
Deliver a runnable server skeleton and establish a safe technical baseline for protocol work.

## Outcome
Sprint 001 is closed with objective achieved and scope over-delivered on architecture.

### Delivered
- Runnable host with tick-driven `GameLoopService`.
- TCP listener and connection lifecycle management.
- Inbound packet framing/parsing for fixed and variable packet lengths.
- Attribute-driven packet metadata (`[PacketHandler]`) with source-generated packet registration.
- Generated opcode constants (`PacketDefinition`) to avoid manual opcode drift.
- Message bus for network thread to game-loop handoff (`IMessageBusService`).
- Domain event bus (`IGameEventBusService`) and first events (`PlayerConnectedEvent`, `PlayerDisconnectedEvent`).
- Session split:
  - `GameNetworkSession` for transport/network state.
  - `GameSession` for gameplay/protocol state.
- Reconnect edge case handled cleanly:
  - 4-byte seed path during `AwaitingSeed`.
- Outbound pipeline extraction and logging coverage alignment.
- Lua scripting integration baseline with bridge from game events to Lua callbacks.
- Embedded HTTP service module (`Moongate.Server.Http`) with health/OpenAPI surface and dedicated HTTP logs.
- Docker image build/run flow with mapped `/app` and `/uo`.
- Expanded tests across networking, packet infrastructure, scripting, and loaders.

### Planned But Not Closed In Sprint 001
- CI AOT publish gate in GitHub workflow remains open and is carried to Sprint 002.

## Scope Delta
The sprint started as `M0 - Skeleton`, but implementation progressed into early `M1/M2` foundations:
- packet registry and generator tooling
- session architecture refinement
- cross-thread message/event infrastructure
- reconnect handshake handling

This delta is accepted because it reduced future integration risk without blocking current execution.

## Verification Snapshot
- Local build and test flow has been exercised repeatedly during the sprint.
- Docker image build flow has been validated with `scripts/build_image.sh`.
- Current detailed technical snapshot is documented in `docs/plans/status-2026-02-18.md`.

## Risks and Technical Debt
- AOT/trim warnings remain in scripting and HTTP minimal API paths.
- Packet coverage is still partial versus full protocol surface.
- Some loaders and protocol behaviors still need deeper integration tests.

## Next Sprint Entry Points
1. Close CI AOT publish gate and keep it green in GitHub Actions.
2. Expand packet coverage for login/game flow and validate lengths/descriptors.
3. Reduce AOT warnings in scripting/HTTP code paths.
4. Continue protocol parity and documentation sync against implemented packets.
