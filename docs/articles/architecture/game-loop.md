# Game Loop System

Timestamp-driven game loop scheduling in Moongate v2.

## Overview

Moongate v2 uses a **timestamp-driven** game loop rather than fixed-sleep tick stepping. This approach provides:

- Deterministic timer behavior
- Adaptive processing based on actual load
- Optional idle CPU throttling
- Stable game state updates

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      Game Loop                               │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌────────────┐    ┌────────────┐    ┌────────────┐         │
│  │  GameLoop  │───▶│  Timer     │───▶│  Message   │         │
│  │  Service   │    │  Wheel     │    │  Bus       │         │
│  └────────────┘    └────────────┘    └────────────┘         │
│         │                                      │             │
│         │                                      ▼             │
│    ┌────┴────┐    ┌────────────┐    ┌────────────┐          │
│    │  Timer  │    │  Packet    │    │  Domain    │          │
│    │  Delta  │    │  Listeners │    │  Events    │          │
│    └─────────┘    └────────────┘    └────────────┘          │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## Core Concepts

### Timestamp-Driven Scheduling

Unlike fixed-sleep loops that tick at constant intervals, Moongate v2 uses monotonic time:

```csharp
public sealed class GameLoopService
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var loopTimestamp = _stopwatch.ElapsedMilliseconds;
            
            // Process one game loop tick
            await ProcessTickAsync(loopTimestamp);
            
            // Optional: throttle idle CPU usage
            if (config.IdleCpuEnabled && !workProcessed)
            {
                await Task.Delay(config.IdleSleepMilliseconds, cancellationToken);
            }
        }
    }
}
```

### Timer Wheel

Efficient timer scheduling using a wheel data structure:

```csharp
public sealed class TimerWheelService
{
    private readonly TimerWheel _wheel;
    private long _accumulatedMilliseconds;
    
    public void UpdateTicksDelta(long elapsedMilliseconds)
    {
        _accumulatedMilliseconds += elapsedMilliseconds;
        
        // Advance wheel based on accumulated time
        var ticksToAdvance = _accumulatedMilliseconds / TickInterval;
        _accumulatedMilliseconds %= TickInterval;
        
        for (int i = 0; i < ticksToAdvance; i++)
        {
            _wheel.Advance();
            FireExpiredTimers();
        }
    }
    
    public string Schedule(TimeSpan interval, Action callback)
    {
        var timerId = Guid.NewGuid().ToString();
        _wheel.Schedule(interval, () => callback());
        return timerId;
    }
}
```

**Benefits:**
- O(1) timer insertion
- Efficient batch expiration
- No heap allocations for timer updates

## Game Loop Tick Processing

Each tick processes:

1. **Network Messages** - Dequeue and handle inbound packets
2. **Timer Events** - Fire expired timers
3. **Domain Events** - Process game events
4. **Session Updates** - Update client sessions

```csharp
private async Task ProcessTickAsync(long loopTimestamp)
{
    var tickStart = Stopwatch.GetTimestamp();
    
    // 1. Process network messages
    while (_messageBus.TryDequeue(out var message))
    {
        await _packetListener.HandleAsync(message);
    }
    
    // 2. Advance timer wheel
    _timerService.UpdateTicksDelta(loopTimestamp - _lastTimestamp);
    _lastTimestamp = loopTimestamp;
    
    // 3. Process domain events
    while (_eventBus.TryDequeue(out var evt))
    {
        await _eventProcessor.ProcessAsync(evt);
    }
    
    // 4. Update sessions
    foreach (var session in _sessionManager.Sessions)
    {
        session.Update();
    }
    
    // Track tick duration
    var tickDuration = Stopwatch.GetElapsedTime(tickStart).TotalMilliseconds;
    _metrics.RecordTickDuration(tickDuration);
}
```

## Configuration

### Idle CPU Throttling

Reduce CPU usage when server is idle:

```json
{
  "game": {
    "idleCpuEnabled": true,
    "idleSleepMilliseconds": 5
  }
}
```

**How it works:**
- If no work was processed in a tick, sleep for configured duration
- Reduces CPU usage from ~100% to ~5% when idle
- Adds minimal latency (5ms default)

### Tick Rate Targeting

Control maximum tick duration:

```json
{
  "game": {
    "tickRateMilliseconds": 100,
    "maxTickDurationMilliseconds": 500
  }
}
```

**Behavior:**
- Target tick duration: 100ms (10 ticks/second)
- Maximum allowed: 500ms (prevent spiral of death)
- If tick exceeds max, log warning and continue

## Metrics

### Exposed Metrics

```
# Game loop metrics
moongate_gameloop_tick_duration_avg_ms
moongate_gameloop_tick_duration_max_ms
moongate_gameloop_loop_work_units_avg
moongate_gameloop_loop_tick_count
moongate_gameloop_loop_idle_sleep_count

# Timer metrics
moongate_timer_timer_processed_ticks_total
```

### Monitoring Tick Health

```csharp
if (tickDuration > _config.MaxTickDurationMilliseconds)
{
    logger.LogWarning("Tick duration {Duration}ms exceeded maximum {Max}ms", 
        tickDuration, _config.MaxTickDurationMilliseconds);
}
```

## Timer System

### Scheduling Timers

```csharp
// Schedule repeating timer
var timerId = timerService.Schedule(
    interval: TimeSpan.FromSeconds(30),
    callback: () => _databaseService.SaveAsync(),
    repeat: true
);

// Schedule one-shot timer
timerService.ScheduleOnce(
    delay: TimeSpan.FromMinutes(5),
    callback: () => SendReminder()
);

// Cancel timer
timerService.Cancel(timerId);
```

### Timer Callbacks

```csharp
public void OnTimerCallback(string timerId)
{
    logger.LogDebug("Executing timer callback {TimerId}", timerId);
    
    try
    {
        // Execute callback
        _callbacks[timerId]();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Timer callback {TimerId} failed", timerId);
    }
}
```

## Threading Model

### Single-Threaded Game Loop

All game state updates happen on a single thread:

```
Network Thread(s)  →  Message Bus  →  Game Loop Thread  →  World State
     (Multiple)                        (Single)
```

**Benefits:**
- No locks on game state
- Deterministic update order
- Easy to reason about

### Cross-Thread Communication

Via message passing only:

```csharp
// Network thread (no direct state mutation)
messageBus.Enqueue(new NetworkMessage
{
    Packet = packet,
    Session = session
});

// Game loop thread (safe state mutation)
while (messageBus.TryDequeue(out var message))
{
    worldState.ApplyMessage(message);
}
```

## Performance Considerations

### Tick Duration Analysis

```
Ideal:     [====10ms====][====10ms====][====10ms====]
Actual:    [====8ms=====][====12ms====][====9ms=====]
           ←----------- 10ms avg ----------→
```

**Key metrics:**
- Average tick duration
- Maximum tick duration
- Standard deviation
- Work units per tick

### Optimization Strategies

1. **Batch Processing**
   - Process multiple messages per tick
   - Reduce per-message overhead

2. **Early Exit**
   - Skip empty ticks quickly
   - Idle CPU throttling

3. **Efficient Data Structures**
   - Timer wheel for O(1) scheduling
   - Concurrent queues for message bus

## Error Handling

### Tick Failures

```csharp
try
{
    await ProcessTickAsync(loopTimestamp);
}
catch (Exception ex)
{
    logger.LogError(ex, "Game loop tick failed");
    // Continue to next tick - don't crash server
}
```

### Timer Failures

```csharp
try
{
    callback();
}
catch (Exception ex)
{
    logger.LogError(ex, "Timer {TimerId} callback failed", timerId);
    // Continue to next timer
}
```

## Testing

### Unit Tests

```csharp
[Fact]
public void TimerWheel_Schedule_TimerFiresAfterInterval()
{
    var wheel = new TimerWheel();
    var fired = false;
    
    wheel.Schedule(TimeSpan.FromMilliseconds(100), () => fired = true);
    
    Thread.Sleep(150);
    wheel.Advance();
    
    Assert.True(fired);
}
```

### Integration Tests

```csharp
[Fact]
public async Task GameLoop_ProcessesMessages()
{
    var loop = new GameLoopService(...);
    var cts = new CancellationTokenSource();
    
    _ = loop.RunAsync(cts.Token);
    
    messageBus.Enqueue(testMessage);
    
    await Task.Delay(100);
    cts.Cancel();
    
    Assert.True(messageProcessed);
}
```

## Next Steps

- **[Event System](events.md)** - Domain events and message bus
- **[Session Management](sessions.md)** - Client session handling
- **[Persistence](../persistence/overview.md)** - Data storage

---

**Previous**: [Network System](network.md) | **Next**: [Event System](events.md)
