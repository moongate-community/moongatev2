# Event System

Domain events and message bus architecture in Moongate v2.

## Overview

Moongate v2 uses an **event-driven architecture** to decouple components:

- **Inbound packets** → Domain logic via message bus
- **Domain events** → Outbound packets via event listeners
- **Strict separation** between network parsing and game logic

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Event System                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Inbound Flow:                                                   │
│  ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐  │
│  │  Packet  │───▶│ Message  │───▶│  Domain  │───▶│  Game    │  │
│  │ Listener │    │   Bus    │    │  Logic   │    │  State   │  │
│  └──────────┘    └──────────┘    └──────────┘    └──────────┘  │
│                                                                  │
│  Outbound Flow:                                                  │
│  ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐  │
│  │  Game    │───▶│  Event   │───▶│ Outbound │───▶│  Packet  │  │
│  │  State   │    │   Bus    │    │ Listener │    │  Queue   │  │
│  └──────────┘    └──────────┘    └──────────┘    └──────────┘  │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

## Message Bus (Inbound)

### IMessageBusService

Bridges network thread and game loop:

```csharp
public interface IMessageBusService
{
    void Enqueue(ReadOnlySpan<byte> packet, GameNetworkSession session);
    bool TryDequeue(out NetworkMessage message);
    int PendingCount { get; }
}
```

### Usage

**Network Thread (Enqueue):**
```csharp
public void HandlePacket(ReadOnlySpan<byte> packet, GameNetworkSession session)
{
    // Don't mutate game state directly!
    // Instead, enqueue for game loop processing
    _messageBus.Enqueue(packet, session);
}
```

**Game Loop (Dequeue):**
```csharp
while (_messageBus.TryDequeue(out var message))
{
    _packetListener.Handle(message.Packet, message.Session);
}
```

### Implementation

```csharp
public sealed class MessageBusService : IMessageBusService
{
    private readonly ConcurrentQueue<NetworkMessage> _queue = new();
    
    public void Enqueue(ReadOnlySpan<byte> packet, GameNetworkSession session)
    {
        // Copy packet data (span is stack-only)
        var packetCopy = packet.ToArray();
        _queue.Enqueue(new NetworkMessage
        {
            Packet = packetCopy,
            Session = session,
            Timestamp = Stopwatch.GetTimestamp()
        });
    }
    
    public bool TryDequeue(out NetworkMessage message)
    {
        return _queue.TryDequeue(out message);
    }
    
    public int PendingCount => _queue.Count;
}
```

## Event Bus (Outbound)

### IGameEventBusService

Publishes domain events to listeners:

```csharp
public interface IGameEventBusService
{
    void Publish<TEvent>(TEvent evt) where TEvent : IGameEvent;
    void RegisterListener<TEvent>(IOutboundEventListener<TEvent> listener);
}

public interface IGameEvent
{
    DateTime Timestamp { get; }
}
```

### Domain Events

```csharp
public sealed class PlayerConnectedEvent : IGameEvent
{
    public DateTime Timestamp { get; init; }
    public Serial PlayerSerial { get; init; }
    public string PlayerName { get; init; }
    public NetworkSession Session { get; init; }
}

public sealed class PlayerDisconnectedEvent : IGameEvent
{
    public DateTime Timestamp { get; init; }
    public Serial PlayerSerial { get; init; }
    public DisconnectReason Reason { get; init; }
}
```

### Outbound Event Listeners

Handle domain events and produce side effects:

```csharp
public interface IOutboundEventListener<TEvent> where TEvent : IGameEvent
{
    int Priority { get; }  // Lower = higher priority
    void Handle(TEvent evt);
}
```

**Example:**
```csharp
[OutboundEventListener(typeof(PlayerConnectedEvent))]
public sealed class PlayerConnectedListener : IOutboundEventListener<PlayerConnectedEvent>
{
    public int Priority => 0;
    
    private readonly IOutgoingPacketQueue _packetQueue;
    
    public void Handle(PlayerConnectedEvent evt)
    {
        // Send welcome packet
        _packetQueue.Enqueue(new SendWelcomePacket(), evt.Session);
        
        // Send season packet
        _packetQueue.Enqueue(new SendSeasonPacket(Season.Spring), evt.Session);
        
        logger.LogInformation("Player {Name} connected", evt.PlayerName);
    }
}
```

### Registration

Events listeners are registered at startup:

```csharp
public static class ServiceCollectionExtensions
{
    public static void RegisterOutboundEventListener<TEvent, TListener>(
        this IServiceCollection services)
        where TEvent : IGameEvent
        where TListener : class, IOutboundEventListener<TEvent>
    {
        services.AddSingleton<IOutboundEventListener<TEvent>, TListener>();
    }
}

// In Program.cs
services.RegisterOutboundEventListener<PlayerConnectedEvent, PlayerConnectedListener>();
```

## Outgoing Packet Queue

### IOutgoingPacketQueue

Queues packets for sending to clients:

```csharp
public interface IOutgoingPacketQueue
{
    void Enqueue<TPacket>(TPacket packet, NetworkSession session) where TPacket : IOutgoingPacket;
    void Flush();
}
```

### Usage

```csharp
public void Handle(PlayerConnectedEvent evt)
{
    _packetQueue.Enqueue(new WelcomePacket(), evt.Session);
    _packetQueue.Enqueue(new SeasonPacket(), evt.Session);
    _packetQueue.Enqueue(new MapMessagePacket(), evt.Session);
}
```

### Implementation

```csharp
public sealed class OutgoingPacketQueue : IOutgoingPacketQueue
{
    private readonly Queue<(IPacket Packet, NetworkSession Session)> _queue = new();
    
    public void Enqueue<TPacket>(TPacket packet, NetworkSession session) 
        where TPacket : IOutgoingPacket
    {
        _queue.Enqueue((packet, session));
    }
    
    public void Flush()
    {
        while (_queue.Count > 0)
        {
            var (packet, session) = _queue.Dequeue();
            session.Send(packet);
        }
    }
}
```

## Packet Listener (Inbound)

### IPacketListener

Handles inbound packets and applies domain logic:

```csharp
public interface IPacketListener
{
    void Handle(ReadOnlySpan<byte> packet, GameNetworkSession session);
}
```

### Example Handler

```csharp
[PacketHandler(0x02, "Move Request")]
public sealed class MoveRequestListener : IPacketListener
{
    private readonly IGameEventBusService _eventBus;
    private readonly IMobileService _mobileService;
    
    public void Handle(ReadOnlySpan<byte> packet, GameNetworkSession session)
    {
        var reader = new SpanReader(packet);
        var sequence = reader.ReadByte();
        var direction = reader.ReadByte();
        
        var mobile = _mobileService.GetBySession(session);
        if (mobile == null) return;
        
        // Apply movement
        var success = _mobileService.Move(mobile, direction);
        
        // Publish event for side effects
        _eventBus.Publish(new MobileMovedEvent
        {
            MobileSerial = mobile.Serial,
            NewPosition = mobile.Position,
            Direction = direction,
            Success = success
        });
    }
}
```

## Event Flow Example

### Complete Flow: Player Connection

```
1. Client connects
   ↓
2. GameNetworkSession created
   ↓
3. Network thread receives packets
   ↓
4. MessageBus.Enqueue(loginPacket)
   ↓
5. Game loop dequeues
   ↓
6. PacketListener.Handle(loginPacket)
   ↓
7. Domain logic validates login
   ↓
8. EventBus.Publish(PlayerConnectedEvent)
   ↓
9. OutboundEventListener handles event
   ↓
10. PacketQueue.Enqueue(welcomePacket)
   ↓
11. PacketQueue.Flush() sends to client
```

## Event Ordering

Events are processed in order:

```csharp
// Events published in this order
_eventBus.Publish(new PlayerConnectedEvent { ... });
_eventBus.Publish(new LoadPlayerDataEvent { ... });
_eventBus.Publish(new SendWorldStateEvent { ... });

// Listeners receive in same order
// Priority determines order within same event type
```

## Priority System

Listeners can specify priority:

```csharp
public sealed class HighPriorityListener : IOutboundEventListener<PlayerConnectedEvent>
{
    public int Priority => -100;  // Higher priority (processed first)
}

public sealed class LowPriorityListener : IOutboundEventListener<PlayerConnectedEvent>
{
    public int Priority => 100;  // Lower priority (processed later)
}
```

## Metrics

### Exposed Metrics

```
# Event bus metrics
moongate_events_published_total{event_type="PlayerConnectedEvent"}
moongate_events_processed_total{event_type="PlayerConnectedEvent"}
moongate_event_bus_queue_depth

# Message bus metrics
moongate_message_bus_enqueued_total
moongate_message_bus_processed_total
moongate_message_bus_queue_depth
```

## Error Handling

### Event Handler Errors

```csharp
public void Publish<TEvent>(TEvent evt)
{
    var listeners = _listeners[typeof(TEvent)];
    
    foreach (var listener in listeners.OrderBy(l => l.Priority))
    {
        try
        {
            listener.Handle(evt);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Event listener {Listener} failed for event {Event}", 
                listener.GetType().Name, typeof(TEvent).Name);
            // Continue to next listener
        }
    }
}
```

### Message Bus Errors

```csharp
while (_messageBus.TryDequeue(out var message))
{
    try
    {
        _packetListener.Handle(message.Packet, message.Session);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to process network message");
        // Continue to next message
    }
}
```

## Testing

### Unit Tests

```csharp
[Fact]
public void EventBus_Publish_CallsListener()
{
    var eventBus = new GameEventBusService();
    var listener = new MockListener();
    
    eventBus.RegisterListener(listener);
    eventBus.Publish(new TestEvent());
    
    Assert.True(listener.WasCalled);
}
```

### Integration Tests

```csharp
[Fact]
public async Task PlayerConnection_SendsWelcomePacket()
{
    // Simulate login packet
    messageBus.Enqueue(loginPacket, session);
    
    // Process one game loop tick
    await gameLoop.ProcessTickAsync();
    
    // Verify welcome packet was sent
    packetQueue.Verify(q => q.Enqueue(It.IsAny<WelcomePacket>(), session));
}
```

## Best Practices

### DO:
- Keep event handlers small and focused
- Use events for cross-component communication
- Process events synchronously in game loop
- Include timestamps in events for debugging

### DON'T:
- Mutate game state from network thread
- Block in event handlers
- Create circular event dependencies
- Use events for high-frequency data (use state instead)

## Next Steps

- **[Session Management](sessions.md)** - Client session handling
- **[Packet System](../networking/packets.md)** - Packet definitions
- **[Persistence](../persistence/overview.md)** - Data storage

---

**Previous**: [Game Loop](game-loop.md) | **Next**: [Session Management](sessions.md)
