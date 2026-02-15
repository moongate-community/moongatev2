# Moongate v2 - Development Plan

> **Stack**: .NET 10 LTS · AOT · MoonSharp (Lua) · No persistence (in-memory)  
> **Type**: Hobby project · Solo developer  
> **Client target**: Classic Client 7.x (ClassicUO compatible)  
> **Created**: 2026-02-15

---

## Technology Decisions (Locked)

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Runtime | .NET 10 LTS | Long-term support, latest AOT improvements |
| Compilation | AOT-ready, JIT during dev | Fast dev iteration, AOT in CI as gate |
| Scripting | MoonSharp (Lua 5.2) | Coroutines, AOT-safe, lightweight, fast |
| Persistence | None (in-memory only) | Remove complexity, add later when needed |
| Networking | System.IO.Pipelines | Proven in v1, zero-copy, backpressure |
| Web API | ASP.NET Core Minimal APIs | Same process, shared DI, zero overhead |
| Client | Classic 7.x only | Single protocol target, ClassicUO support |

---

## Milestone Plan

The plan follows a **vertical slice** approach: each milestone produces something visible
and testable. No milestone should take more than 2-3 weeks of hobby time. If a milestone
drags past 3 weeks, it is too big — split it.

### M0 - Skeleton (Week 1)

**Goal**: Solution compiles, runs, does nothing useful, but the architecture is in place.

**Tasks**:
- Create solution structure (see Project Layout below)
- Setup `Directory.Build.props` with .NET 10, nullable, AOT-ready flags
- Empty `GameLoop` class with tick loop (prints tick count to console)
- Empty `NetworkService` with TCP listener (accepts connections, logs, disconnects)
- GitHub repo, CI pipeline (build + test + AOT publish)
- `.editorconfig`, `.gitignore`, `README.md`

**Done when**: `dotnet run` starts a process that listens on port 2593 and ticks to console.
AOT publish succeeds in CI.

**Estimated effort**: 1 evening.

---

### M1 - TCP Handshake (Week 1-2)

**Goal**: A UO client connects and sees the server list.

**Tasks**:
- Implement `PacketRegistry` with fixed/variable packet size lookup
- Implement `ProtocolReader` using `PipeReader` for packet framing
- Implement login encryption seed (packet `0xEF`)
- Handle `0x80` Account Login -> respond with server list (`0xA8`)
- Handle `0xA0` Server Select -> respond with server redirect (`0x8C`)
- Handle `0x91` Game Login -> respond with character list (`0xA9`)
- `PacketReader` / `PacketWriter` for zero-allocation read/write

**Done when**: ClassicUO connects, sees server list, selects server, sees (empty) character list.

**Estimated effort**: 3-4 sessions.

**Reference packets**: 0xEF, 0x80, 0xA8, 0xA0, 0x8C, 0x91, 0xA9

---

### M2 - Enter World (Week 2-3)

**Goal**: A character enters the world and stands still.

**Tasks**:
- Implement in-memory `Account` and `Mobile` entities
- Handle `0x5D` Character Select (or `0x00` Character Create - pick one, simplest first)
- Send login confirmation (`0x1B`), map change (`0xBF` sub `0x08`), map patches
- Send `0x20` Draw Player, `0x78` Mobile Incoming (self)
- Send `0x4F` Light level, `0x4E` Sound, `0xBC` Season
- Handle `0xBD` Client Version, `0xBE` Assist Version
- Implement `ClientSession` state machine (Login -> ServerSelect -> GameLogin -> InWorld)

**Done when**: Character appears in the world (standing at a fixed position). Client
shows the game view without crashing.

**Estimated effort**: 4-5 sessions.

**Reference packets**: 0x5D, 0x1B, 0xBF, 0x20, 0x78, 0x4F, 0x4E, 0xBC, 0xBD, 0xBE

---

### M3 - Movement (Week 3-4)

**Goal**: The character walks around Britannia.

**Tasks**:
- Load map data from `.mul` files (map tiles + statics)
- Implement sector grid (16x16 tiles per sector)
- Handle `0x02` Move Request -> validate against map -> send `0x22` Move ACK or `0x21` Deny
- Position tracking per mobile
- Send movement updates to other connected clients in range
- Basic view range management (18 tiles)

**Done when**: Player walks around, sees terrain. Two clients connected see each other move.

**Estimated effort**: 5-6 sessions.

**Reference packets**: 0x02, 0x22, 0x21

---

### M4 - Items & Containers (Week 4-6)

**Goal**: Items exist in the world, player has a backpack, can pick up and drop items.

**Tasks**:
- Implement in-memory `Item` entity with Serial, ItemID, Hue, Position, Container
- Implement `Serial` generator (unique IDs for all entities)
- Send `0x1A` Object Info for world items (or `0xF3` Object Info Enhanced)
- Implement container system (backpack as root container)
- Handle `0x07` Pick Up Item, `0x08` Drop Item
- Send `0x3C` Container Contents, `0x24` Open Container (gump)
- Paperdoll: handle `0x09` Single Click, send `0x11` Status Bar, `0xBF` sub `0x19`

**Done when**: Player opens backpack, sees items, can pick up and drop items between
world and backpack. Paperdoll opens.

**Estimated effort**: 6-8 sessions.

---

### M5 - Chat & Commands (Week 6-7)

**Goal**: Players can talk, admin commands work.

**Tasks**:
- Handle `0xAD` Unicode Speech -> broadcast to nearby players
- Send `0xAE` Unicode Speech (or `0x1C` ASCII Speech)
- Implement command parser (prefix `.` or `[`) 
- Basic commands: `.where` (show coords), `.spawn` (create item), `.go` (teleport)
- Permission system (admin level per account)

**Done when**: Players see each other's chat. Admin can teleport and spawn items via commands.

**Estimated effort**: 3-4 sessions.

---

### M6 - MoonSharp Integration (Week 7-9)

**Goal**: Lua scripts can hook into game events and modify behavior.

**Tasks**:
- Integrate MoonSharp engine with sandbox configuration
- Define Lua API surface: `world.spawn_item()`, `mobile.say()`, `events.on("speech", fn)`
- Implement event bus: game events (OnSpeech, OnMove, OnPickUp) trigger Lua hooks
- Hot-reload: file watcher reloads scripts on change
- Create example scripts: custom command, item interaction, NPC greeting
- Lua coroutine support for delayed actions (wait N ticks then do X)

**Done when**: A Lua script can react to player speech, spawn an item, and display a message.
Scripts reload without server restart.

**Estimated effort**: 6-8 sessions.

---

### M7 - Tooltips & Visual Polish (Week 9-10)

**Goal**: Items and mobiles show tooltip information, equip system works.

**Tasks**:
- Implement MegaCliloc tooltip system (`0xD6` request, `0xD6` response)
- Cliloc file loading (string table lookup)
- Equipment: handle `0x13` Equip Item -> update paperdoll
- Send equipped items in `0x78` Mobile Incoming
- Item properties: weight, name, layer
- `0xBF` sub `0x10` Object Property List hash for caching

**Done when**: Hovering over items/players shows tooltips. Equipment shows on paperdoll.

**Estimated effort**: 5-6 sessions.

---

### Future Milestones (Backlog - not planned in detail yet)

- **M8** - Timer system + skill framework (skill check, gain, cap)
- **M9** - NPC spawning + basic AI (wander, aggro, return home)
- **M10** - Combat skeleton (hit/miss, damage, death, resurrection)
- **M11** - Persistence (add when M0-M10 are solid)
- **M12** - Gumps (server-sent UI dialogs)
- **M13** - Vendor system (buy/sell)
- **M14** - Housing foundation

---

## Project Layout

```text
Moongate.sln
├── src/
│   ├── Moongate.Core/            # Entities, interfaces, enums, serials
│   ├── Moongate.Network/         # TCP, pipelines, encryption, packet framing
│   ├── Moongate.Network.Packets/ # Packet definitions, handlers, registry
│   ├── Moongate.World/           # Map loading, sectors, entity spatial tracking
│   ├── Moongate.GameLogic/       # Movement validation, commands, interactions
│   ├── Moongate.Scripting/       # MoonSharp bridge, Lua API, event hooks
│   └── Moongate.Server/          # Host, DI, startup, Minimal APIs, game loop
│
├── tests/
│   ├── Moongate.Core.Tests/
│   ├── Moongate.Network.Tests/   # Packet serialization golden-file tests
│   └── Moongate.Integration.Tests/
│
├── scripts/                      # Lua game scripts
│   ├── core/                     # Engine-provided base scripts
│   └── custom/                   # Shard customizations
│
├── docs/                         # Protocol notes and dev journal
│   ├── protocol/
│   └── journal/
│
└── tools/
    └── Moongate.PacketSniffer/   # Dev tool for debugging
```

---

## Staying On Track - Anti-Abandonment System

These are strategies specifically designed for the pattern of starting strong and losing
momentum when hitting minor obstacles. Use them deliberately.

### Rule 1: The 30-Minute Rule

If stuck on something for more than 30 minutes, STOP and do one of:
- Skip it with a `// TODO: [describe what's needed]` and move to the next task
- Write a failing test that captures what you want, move on
- Downgrade: implement the simplest possible version that unblocks you

**The goal is forward motion, not perfection.** You can always come back.

### Rule 2: Dev Journal (5 minutes, every session)

At the end of each coding session, write 3-5 lines in `docs/journal/`:

```text
## 2026-02-15

- Implemented PacketRegistry with fixed/variable lookup
- Stuck on encryption seed - client sends different format than POL docs say
- NEXT: capture real packet with Razor to verify format
```

This serves three purposes:
1. You know exactly where to pick up next time
2. You see visible progress over time (motivation)
3. When you skip a week, you don't lose context

### Rule 3: One Session = One Visible Result

Every coding session should end with something you can SEE working. Not refactoring,
not "cleaned up the architecture", not "read about how X works". Something that changes
what happens when you run the server and connect a client.

If a task can't produce a visible result in one session, break it into smaller pieces.

### Rule 4: The Packet-First Rule

When starting a new feature, always begin with: "What packets does this need?"
1. Look up the packets in POL docs / ModernUO source
2. Write the packet reader/writer
3. Write a test with hardcoded bytes
4. Implement the game logic behind it

This gives natural structure and a testable starting point.

### Rule 5: Weekly Self-Check

Every Sunday, answer these three questions (in the journal):

1. **What milestone am I on?** (M0, M1, M2...)
2. **Am I on track?** (yes / behind by N days / stuck)
3. **What is the ONE thing blocking me?** (and: is it actually important?)

If the answer to #3 is "nothing, I just haven't opened the project" - that is the real
blocker. Open the project, run it, and do ONE small thing. Momentum is a physical force.

### Rule 6: The 80% Milestone

A milestone is DONE at 80%. Not 100%. Not perfect. If the character can walk around but
occasionally clips through a wall, that is M3 DONE. File a bug, move to M4. Come back
to the wall clipping in a polish pass after M7.

Perfection is the enemy of shipped milestones.

### Rule 7: Never Rewrite What Works

If a system works but is ugly, leave it. Put a `// REFACTOR:` comment. Rewriting working
code before you have gameplay is the #1 hobby project killer. Refactoring is a reward you
earn after reaching a milestone, not a prerequisite for the next one.

---

## Session Checklist (Copy this into every journal entry)

```text
Session: YYYY-MM-DD
Duration: ~Xh
Milestone: MX

[ ] Visible result achieved?
[ ] Tests written/passing?
[ ] Journal updated?
[ ] Next session's first task clear?

What I did:
-

What's next:
-

Blockers:
-
```

---

## Reference Links

| Resource | URL | Use |
|----------|-----|-----|
| POL Packet Docs | docs.polserver.com/packets | Packet layout reference |
| RuOSI Guide (XML) | ruosi.org/packetguide/index.xml | SA-era packets |
| Jerrith's Guide | uo.torfo.org/packetguide | Historical quirks |
| ModernUO Source | github.com/modernuo/ModernUO | Ground truth for 7.x |
| ClassicUO Packets | github.com/ClassicUO/packets | Non-standard CUO packets |
| uoproxy-runuo Wiki | github.com/credzba/uoproxy-runuo/wiki | Quick packet length table |
| Moongate v1 | github.com/moongate-community/moongate | Previous implementation |
| MoonSharp Docs | moonsharp.org | Scripting engine reference |
