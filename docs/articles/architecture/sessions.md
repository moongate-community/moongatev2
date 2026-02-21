# Session Management

Moongate v2 keeps transport and gameplay concerns separated.

## Session Types

## `GameNetworkSession`

Transport-level state:

- `SessionId`
- `Client` (`MoongateTCPClient`)
- `RemoteEndPoint`
- protocol `State`
- pending bytes buffer
- seed/account/auth flags
- compression/encryption flags

Operations:

- `SetState(...)`
- `SetSeed(...)`
- `MarkAuthenticated(...)`
- `EnterGame(...)`
- compression/encryption toggles

## `GameSession`

Gameplay/protocol context tied to `GameNetworkSession`:

- `SessionId`
- `ClientVersion`
- `AccountId`
- `CharacterId`
- `Character` (`UOMobileEntity`)
- movement and ping state (`MoveSequence`, `MoveTime`, `MoveCredit`, `PingSequence`)

## Session Store Service

`IGameNetworkSessionService` / `GameNetworkSessionService`:

- `GetOrCreate(MoongateTCPClient client)`
- `TryGet(long sessionId, out GameSession session)`
- `GetAll()` snapshot
- `Remove(...)`, `Clear()`, `Count`

Storage is a concurrent dictionary keyed by connection session id.

## Lifecycle

1. client connects
2. `NetworkService` calls `GetOrCreate(...)`
3. transport state progresses through handshake/auth/game states
4. disconnect removes session from store

## Why This Split

- transport concerns (socket/middleware/protocol framing) stay isolated
- game logic operates on `GameSession`
- easier testing and future protocol transitions

---

**Previous**: [Game Loop](game-loop.md) | **Next**: [Event System](events.md)
