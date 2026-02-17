namespace Moongate.Server.Data.Session;

public readonly record struct ClientSession(
    long SessionId,
    string? RemoteEndPoint
);
