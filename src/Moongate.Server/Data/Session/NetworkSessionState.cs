namespace Moongate.Server.Data.Session;

/// <summary>
/// Represents the protocol/lifecycle state of a network game session.
/// </summary>
public enum NetworkSessionState : byte
{
    Connected = 0,
    Login = 1,
    Authenticated = 2,
    InGame = 3,
    Disconnecting = 4,
    Disconnected = 5
}
