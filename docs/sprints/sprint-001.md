# Sprint 001 - M0 Skeleton

Kanban board: [[sprint-001-kanban|Sprint 001 - Kanban]]
Closeout: [[sprint-001-closeout-2026-02-18|Sprint 001 - Closeout (2026-02-18)]]

## Sprint Goal
Get a runnable server skeleton with a visible game loop and TCP listener, plus CI validation including AOT publish.

## Scope
- Milestone target: `M0 - Skeleton`
- Timebox: 1 week (or 1 focused evening)
- Status: Closed (objective achieved, CI AOT gate carried to Sprint 002)

## Sprint Backlog
- [x] Verify solution layout matches the target structure in the development plan ✅ 2026-02-15
- [x] Confirm `Directory.Build.props` is aligned with .NET 10 + nullable + AOT-ready settings ✅ 2026-02-15
- [x] Create `GameLoop` with a basic tick loop and queue processing ✅ 2026-02-17
- [x] Create `NetworkService` with TCP listener on port `2593` ✅ 2026-02-15
- [x] Ensure accepted connections are logged and disconnected cleanly ✅ 2026-02-15
- [x] Add/update `README.md` with run instructions ✅ 2026-02-18
- [ ] Add CI workflow: build, test, and AOT publish gate (AOT gate still missing, moved to Sprint 002)

## Definition of Done
- `dotnet run` starts the server
- Game loop is running and processing packet queues
- TCP listener is active on port `2593`
- CI passes build + test (AOT publish gate pending and explicitly carried over)

## Risks
- AOT publish issues caused by package/runtime incompatibilities

## Out of Scope
- Handshake packets (`M1`)
- Character flow and world entry (`M2+`)
- Persistence and scripting

## Notes
- Keep this sprint at ~80% completion quality to preserve momentum.
- If blocked for more than 30 minutes, ship a minimal fallback and continue.
- Current implementation is already beyond M0 skeleton in networking internals:
  packet registry/source gen, session model split, message bus, and domain events are in place.
- Closeout details and carry-over rationale are captured in
  [[sprint-001-closeout-2026-02-18|Sprint 001 - Closeout (2026-02-18)]].
