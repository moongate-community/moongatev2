# Game Loop System

The game loop runs on a dedicated background thread and drives packet dispatch, timers, and outbound flush.

## Loop Responsibilities

Per iteration, `GameLoopService` does:

1. `DrainPacketQueue()`
2. `_timerService.UpdateTicksDelta(timestampMilliseconds)`
3. `DrainOutgoingPacketQueue()`

If idle CPU throttling is enabled and no work was done, it sleeps for `IdleSleepMilliseconds`.

## Timing Model

- Loop timestamp is derived from `Stopwatch.GetTimestamp()` converted to milliseconds.
- Timer progression is delta-based (not fixed-sleep-tick based).
- `TimerWheelService` accumulates elapsed milliseconds and processes due slots.

## Timer Wheel

`TimerWheelService` features:

- hashed wheel buckets
- named timers
- register/unregister by id and by name
- repeating and one-shot timers
- callback execution metrics and error counting

Used by persistence for autosave timer `db_save`.

## Outbound Flush

Outbound network send is intentionally inside the game loop:

- dequeue `OutgoingGamePacket`
- resolve session from `IGameNetworkSessionService`
- call `IOutboundPacketSender.Send(...)`

This keeps outbound ordering tied to loop progression.

## Metrics

`GameLoopService` exposes:

- tick count
- uptime
- average/max tick duration
- idle sleep count
- average work units
- outbound queue depth
- total outbound packets

Timer metrics are exposed separately by `ITimerMetricsSource`.

---

**Previous**: [Network System](network.md) | **Next**: [Session Management](sessions.md)
