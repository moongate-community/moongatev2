# Sprint 001 - M0 Skeleton

## Sprint Goal
Get a runnable server skeleton with a visible game loop and TCP listener, plus CI validation including AOT publish.

## Scope
- Milestone target: `M0 - Skeleton`
- Timebox: 1 week (or 1 focused evening)
- Status: In Progress

## Sprint Backlog
- [ ] Verify solution layout matches the target structure in the development plan
- [ ] Confirm `Directory.Build.props` is aligned with .NET 10 + nullable + AOT-ready settings
- [ ] Create `GameLoop` with a basic tick loop and console tick output
- [ ] Create `NetworkService` with TCP listener on port `2593`
- [ ] Ensure accepted connections are logged and disconnected cleanly
- [ ] Add/update `README.md` with run instructions
- [ ] Add CI workflow: build, test, and AOT publish gate

## Definition of Done
- `dotnet run` starts the server
- Tick output is visible in console
- TCP listener is active on port `2593`
- CI passes build + test + AOT publish

## Risks
- .NET 10 preview/SDK mismatch across local and CI environments
- AOT publish issues caused by package/runtime incompatibilities

## Out of Scope
- Handshake packets (`M1`)
- Character flow and world entry (`M2+`)
- Persistence and scripting

## Notes
- Keep this sprint at ~80% completion quality to preserve momentum.
- If blocked for more than 30 minutes, ship a minimal fallback and continue.
