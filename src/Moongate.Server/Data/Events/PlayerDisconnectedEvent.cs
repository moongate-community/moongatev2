namespace Moongate.Server.Data.Events;

/// <summary>
/// Event emitted when a client session disconnects from the server.
/// </summary>
public readonly record struct PlayerDisconnectedEvent(
    long SessionId,
    string? RemoteEndPoint,
    long Timestamp
) : IGameEvent;
