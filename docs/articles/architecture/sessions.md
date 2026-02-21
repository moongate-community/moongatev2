# Session Management

Client session handling in Moongate v2.

## Overview

Moongate v2 uses a **dual-session model**:

- **GameNetworkSession**: Transport-level TCP connection
- **GameSession**: Gameplay-level protocol context

This separation allows clean boundaries between networking and game logic.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Session Management                        │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────────┐              ┌──────────────────┐     │
│  │ GameNetwork      │  1:1 or 1:0  │ Game             │     │
│  │ Session          │──────────────│ Session          │     │
│  │                  │              │                  │     │
│  │ - Socket         │              │ - Player Mobile  │     │
│  │ - Serial         │              │ - Account        │     │
│  │ - State          │              │ - Flags          │     │
│  │ - Buffers        │              │ - Permissions    │     │
│  └──────────────────┘              └──────────────────┘     │
│           │                                     │            │
│           ▼                                     ▼            │
│  ┌──────────────────┐              ┌──────────────────┐     │
│  │ Network Thread   │              │ Game Loop        │     │
│  │ (Packet I/O)     │              │ (Game Logic)     │     │
│  └──────────────────┘              └──────────────────┘     │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## GameNetworkSession

### Transport-Level Session

Represents a TCP connection to a client:

```csharp
public sealed class GameNetworkSession : IDisposable
{
    public Serial Serial { get; }           // Unique session ID
    public Socket Socket { get; }           // TCP socket
    public SessionState State { get; set; } // Connection state
    
    private GameSession? _gameSession;      // Linked gameplay session
    private readonly byte[] _receiveBuffer;
    private readonly NetworkBufferPool _bufferPool;
    
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && State == SessionState.Connected)
            {
                var received = await Socket.ReceiveAsync(_receiveBuffer, cancellationToken);
                if (received == 0)
                {
                    State = SessionState.Disconnected;
                    break;
                }
                
                ProcessReceivedData(_receiveBuffer.AsSpan(0, received));
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (SocketException ex)
        {
            logger.LogError(ex, "Socket error on session {Serial}", Serial);
            State = SessionState.Disconnected;
        }
        finally
        {
            Dispose();
        }
    }
    
    public void LinkGameSession(GameSession gameSession)
    {
        _gameSession = gameSession;
    }
    
    public void Send(ReadOnlySpan<byte> packet)
    {
        if (State != SessionState.Connected) return;
        
        _ = Socket.SendAsync(packet, SocketFlags.None);
    }
}
```

### Session States

```csharp
public enum SessionState
{
    Connecting,     // TCP connection established
    Connected,      // Ready for packets
    InGame,         // Player logged in
    Disconnecting,  // Shutdown initiated
    Disconnected    // Connection closed
}
```

### Session Lifecycle

```
Connecting → Connected → InGame → Disconnecting → Disconnected
                  ↑            ↓
                  └────────────┘ (error path)
```

## GameSession

### Gameplay-Level Session

Represents a player's gameplay context:

```csharp
public sealed class GameSession
{
    public Serial Serial { get; }           // Unique session ID
    public Serial? PlayerSerial { get; set; } // Linked player mobile
    public Account? Account { get; set; }   // Linked account
    public SessionFlags Flags { get; set; } // Session flags
    
    public GameNetworkSession NetworkSession { get; }
    
    // Player state
    public bool IsInWorld { get; set; }
    public bool IsMoving { get; set; }
    public DateTime LastActivity { get; set; }
    
    // Permissions
    public bool IsAdmin { get; set; }
    public bool IsModerator { get; set; }
    
    public void OnPacketReceived(ReadOnlySpan<byte> packet)
    {
        LastActivity = DateTime.UtcNow;
        // ... process packet
    }
    
    public void SendPacket<TPacket>(TPacket packet) where TPacket : IOutgoingPacket
    {
        NetworkSession.Send(packet.Serialize());
    }
}
```

### Session Flags

```csharp
[Flags]
public enum SessionFlags
{
    None = 0,
    EncryptionEnabled = 1 << 0,
    CompressionEnabled = 1 << 1,
    PacketLogging = 1 << 2,
    AwaitingResponse = 1 << 3
}
```

## Session Manager

### ISessionManager

Manages all active sessions:

```csharp
public interface ISessionManager
{
    GameNetworkSession CreateSession(Socket socket);
    void RemoveSession(Serial serial);
    GameSession? GetGameSession(Serial serial);
    IReadOnlyCollection<GameSession> GetAllSessions();
    int ActiveCount { get; }
}
```

### Implementation

```csharp
public sealed class SessionManager : ISessionManager
{
    private readonly ConcurrentDictionary<Serial, GameNetworkSession> _networkSessions = new();
    private readonly ConcurrentDictionary<Serial, GameSession> _gameSessions = new();
    private readonly SerialGenerator _serialGenerator = new();
    
    public GameNetworkSession CreateSession(Socket socket)
    {
        var serial = _serialGenerator.Next();
        var session = new GameNetworkSession(serial, socket, ...);
        
        _networkSessions[serial] = session;
        _ = session.RunAsync(_cancellationToken);
        
        logger.LogDebug("Created network session {Serial}", serial);
        return session;
    }
    
    public void RemoveSession(Serial serial)
    {
        if (_networkSessions.TryRemove(serial, out var networkSession))
        {
            networkSession.Dispose();
            logger.LogDebug("Removed network session {Serial}", serial);
        }
        
        // Also remove linked game session
        if (_gameSessions.TryRemove(serial, out var gameSession))
        {
            CleanupGameSession(gameSession);
        }
    }
    
    public GameSession? GetGameSession(Serial serial)
    {
        return _gameSessions.GetValueOrDefault(serial);
    }
    
    public IReadOnlyCollection<GameSession> GetAllSessions()
    {
        return _gameSessions.Values.ToList();
    }
    
    public int ActiveCount => _gameSessions.Count;
}
```

## Session Creation Flow

### New Connection

```csharp
// 1. TCP server accepts connection
var tcpClient = await _listener.AcceptTcpClientAsync(cancellationToken);

// 2. Create network session
var networkSession = _sessionManager.CreateSession(tcpClient.Client);

// 3. Session starts receive loop
_ = networkSession.RunAsync(cancellationToken);
```

### Player Login

```csharp
// 1. Receive login packet
[PacketHandler(0x80, "Login Request")]
public void HandleLogin(ReadOnlySpan<byte> packet, GameNetworkSession networkSession)
{
    // 2. Validate credentials
    var account = _accountService.Validate(username, password);
    if (account == null)
    {
        SendLoginFailed(networkSession);
        return;
    }
    
    // 3. Create game session
    var gameSession = new GameSession
    {
        Serial = networkSession.Serial,
        Account = account,
        NetworkSession = networkSession
    };
    
    // 4. Link sessions
    networkSession.LinkGameSession(gameSession);
    _sessionManager.RegisterGameSession(gameSession);
    
    // 5. Publish event
    _eventBus.Publish(new PlayerConnectedEvent
    {
        Session = gameSession,
        Account = account
    });
}
```

## Session Cleanup

### Normal Disconnect

```csharp
public void Disconnect(GameSession session, DisconnectReason reason)
{
    // 1. Publish disconnect event
    _eventBus.Publish(new PlayerDisconnectedEvent
    {
        Session = session,
        Reason = reason
    });
    
    // 2. Save player state
    _persistenceService.SavePlayer(session.PlayerSerial);
    
    // 3. Remove sessions
    _sessionManager.RemoveSession(session.Serial);
    
    // 4. Close socket
    session.NetworkSession.Socket.Close();
}
```

### Timeout Handling

```csharp
public void CheckSessionTimeouts()
{
    var timeout = TimeSpan.FromMinutes(30);
    var now = DateTime.UtcNow;
    
    foreach (var session in _sessionManager.GetAllSessions())
    {
        if (now - session.LastActivity > timeout)
        {
            logger.LogInformation("Session {Serial} timed out", session.Serial);
            Disconnect(session, DisconnectReason.Timeout);
        }
    }
}
```

## Packet Handling per Session

### Per-Session State

```csharp
public void HandlePacket(ReadOnlySpan<byte> packet, GameNetworkSession networkSession)
{
    var gameSession = networkSession.GameSession;
    
    if (gameSession == null)
    {
        // Only allow login packets before game session is created
        if (packet[0] != 0x80)  // Not a login packet
        {
            networkSession.Disconnect("Protocol violation");
            return;
        }
    }
    
    // Process packet with session context
    _packetListener.Handle(packet, networkSession);
}
```

## Security

### Session Validation

```csharp
public void ValidatePacket(ReadOnlySpan<byte> packet, GameNetworkSession session)
{
    // Check session state
    if (session.State != SessionState.Connected && session.State != SessionState.InGame)
    {
        session.Disconnect("Invalid session state");
        return;
    }
    
    // Check for flood
    if (session.PacketsReceivedThisSecond > MaxPacketsPerSecond)
    {
        session.Disconnect("Packet flood detected");
        return;
    }
    
    session.PacketsReceivedThisSecond++;
}
```

### Session Encryption

```csharp
public void EnableEncryption(GameSession session)
{
    session.Flags |= SessionFlags.EncryptionEnabled;
    // Initialize encryption for this session
}

public byte[] EncryptPacket(byte[] packet, GameSession session)
{
    if ((session.Flags & SessionFlags.EncryptionEnabled) == 0)
    {
        return packet;  // No encryption
    }
    
    return _encryptionService.Encrypt(packet, session.EncryptionKey);
}
```

## Metrics

### Exposed Metrics

```
# Session metrics
moongate_sessions_active_total
moongate_sessions_created_total
moongate_sessions_disconnected_total{reason="timeout"}
moongate_sessions_disconnected_total{reason="error"}

# Per-session stats (sampled)
moongate_session_packets_received{session_serial="0x00000001"}
moongate_session_packets_sent{session_serial="0x00000001"}
moongate_session_duration_seconds{session_serial="0x00000001"}
```

## Logging

### Session Events

```csharp
logger.LogInformation("Session {Serial} created from {EndPoint}", 
    session.Serial, socket.RemoteEndPoint);

logger.LogDebug("Session {Serial} linked to player {PlayerSerial}", 
    session.Serial, gameSession.PlayerSerial);

logger.LogInformation("Session {Serial} disconnected: {Reason}", 
    session.Serial, disconnectReason);
```

### Packet Logging

```csharp
if (config.LogPacketData)
{
    logger.LogDebug("Session {Serial} ← Packet 0x{PacketId:X2} ({Length} bytes)", 
        session.Serial, packet[0], packet.Length);
}
```

## Testing

### Unit Tests

```csharp
[Fact]
public void SessionManager_CreateSession_AddsToDictionary()
{
    var manager = new SessionManager();
    using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    
    var session = manager.CreateSession(socket);
    
    Assert.NotNull(manager.GetNetworkSession(session.Serial));
}
```

### Integration Tests

```csharp
[Fact]
public async Task Session_Login_CreatesGameSession()
{
    // Connect client
    var client = await ConnectClientAsync();
    
    // Send login packet
    await client.SendAsync(loginPacket);
    
    // Process game loop tick
    await gameLoop.ProcessTickAsync();
    
    // Verify game session created
    var session = sessionManager.GetGameSession(client.Serial);
    Assert.NotNull(session);
}
```

## Next Steps

- **[Event System](events.md)** - Domain events
- **[Persistence](../persistence/overview.md)** - Data storage
- **[Networking](../networking/packets.md)** - Packet handling

---

**Previous**: [Event System](events.md) | **Next**: [Solution Structure](solution.md)
