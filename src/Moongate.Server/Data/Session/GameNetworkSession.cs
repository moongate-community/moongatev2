using Moongate.Network.Client;

namespace Moongate.Server.Data.Session;

/// <summary>
/// Represents server-side state for a connected network client session.
/// </summary>
public sealed class GameNetworkSession
{
    private readonly List<byte> _pendingBytes = [];

    public GameNetworkSession(MoongateTCPClient client)
    {
        SessionId = client.SessionId;
        RemoteEndPoint = client.RemoteEndPoint?.ToString();
    }

    /// <summary>
    /// Gets the unique connection session identifier.
    /// </summary>
    public long SessionId { get; }

    /// <summary>
    /// Gets the latest known remote endpoint string.
    /// </summary>
    public string? RemoteEndPoint { get; private set; }

    /// <summary>
    /// Updates endpoint information from current client state.
    /// </summary>
    /// <param name="client">Source TCP client.</param>
    public void Refresh(MoongateTCPClient client)
    {
        RemoteEndPoint = client.RemoteEndPoint?.ToString();
    }

    /// <summary>
    /// Executes an action while holding the session pending-bytes lock.
    /// </summary>
    /// <param name="action">Action that can inspect and mutate pending bytes.</param>
    public void WithPendingBytesLock(Action<List<byte>> action)
    {
        lock (_pendingBytes)
        {
            action(_pendingBytes);
        }
    }
}
