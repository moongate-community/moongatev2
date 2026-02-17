namespace Moongate.Server.Data.Events;

/// <summary>
/// Event emitted when a new client session connects to the server.
/// </summary>
public readonly record struct PlayerConnectedEvent(
    long SessionId,
    string? RemoteEndPoint,
    long Timestamp
) : IGameEvent;
