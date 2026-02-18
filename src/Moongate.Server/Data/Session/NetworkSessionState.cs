namespace Moongate.Server.Data.Session;

/// <summary>
/// Represents the protocol/lifecycle state of a network game session.
/// </summary>
public enum NetworkSessionState : byte
{
    Connected = 0,
    AwaitingSeed = 1,
    Login = 2,
    Authenticated = 3,
    InGame = 4,
    Disconnecting = 5,
    Disconnected = 6
}
