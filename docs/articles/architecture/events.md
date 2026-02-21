# Event System

Moongate v2 uses two distinct async messaging paths:

- packet bus for network-to-game-loop traffic
- game event bus for domain event fan-out

## Inbound Packet Bus

`IMessageBusService` stores `IncomingGamePacket` in a channel:

- producer: `NetworkService.PublishIncomingPacket(...)`
- consumer: `GameLoopService.DrainPacketQueue()`

Properties:

- single reader, multi writer
- explicit queue depth metric (`CurrentQueueDepth`)
- no game-state mutation on transport callback

## Packet Dispatch

`IPacketDispatchService` maps opcode to one or more `IPacketListener`.

- listeners are registered at bootstrap (`BootstrapPacketHandlerRegistration`)
- dispatch uses parsed packet instances (`IGameNetworkPacket`)
- missing listeners are logged with opcode + descriptor description

Base helper:

- `BasePacketListener` provides common pattern and protected enqueue helpers for outbound packets.

## Game Event Bus

`IGameEventBusService` (`GameEventBusService`) provides publish/subscribe by generic event type.

- typed listener: `IGameEventListener<TEvent>`
- optional global listener: `IGameEventListener<IGameEvent>`
- listeners are invoked sequentially per publish call
- listener exceptions are caught and logged

## Outbound Event Listeners

The codebase includes outbound-event listener contracts (`IOutboundEventListener<TEvent>`) and base class (`BaseOutboundEventListener<TEvent>`) for event-driven packet emission.

Typical pattern:

1. domain service publishes `IGameEvent`
2. outbound listener reacts
3. listener enqueues network packet(s) into `IOutgoingPacketQueue`
4. game loop flushes queue via `IOutboundPacketSender`

## Practical Rule

- Use message bus for cross-thread transport handoff.
- Use game event bus for decoupled domain behaviors inside server flow.

---

**Previous**: [Sessions](sessions.md) | **Next**: [Architecture Overview](overview.md)
