using Moongate.Network.Client;

namespace Moongate.Server.Data.Session;

/// <summary>
/// Represents server-side state for a connected network client session.
/// </summary>
public sealed class GameNetworkSession
{
    private readonly List<byte> _pendingBytes = [];
    private readonly object _stateSync = new();
    private MoongateTCPClient? _client;

    public GameNetworkSession(MoongateTCPClient client)
    {
        SessionId = client.SessionId;
        _client = client;
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
    /// Gets the currently associated TCP client, if still attached.
    /// </summary>
    public MoongateTCPClient? Client => Volatile.Read(ref _client);

    /// <summary>
    /// Gets the current protocol state of the session.
    /// </summary>
    public NetworkSessionState State { get; private set; } = NetworkSessionState.Connected;

    /// <summary>
    /// Gets whether outbound/inbound compression is enabled for this session.
    /// </summary>
    public bool CompressionEnabled { get; private set; }

    /// <summary>
    /// Gets whether transport encryption is enabled for this session.
    /// </summary>
    public bool EncryptionEnabled { get; private set; }

    /// <summary>
    /// Gets the authenticated account id, when available.
    /// </summary>
    public long? AccountId { get; private set; }

    /// <summary>
    /// Gets the authenticated account name, when available.
    /// </summary>
    public string? AccountName { get; private set; }

    /// <summary>
    /// Gets the selected in-game character serial, when available.
    /// </summary>
    public uint? CharacterSerial { get; private set; }

    /// <summary>
    /// Detaches the underlying TCP client from this session.
    /// </summary>
    public void DetachClient()
    {
        _client = null;
        SetState(NetworkSessionState.Disconnected);
    }

    /// <summary>
    /// Disables compression for this session.
    /// </summary>
    public void DisableCompression()
    {
        lock (_stateSync)
        {
            CompressionEnabled = false;
        }
    }

    /// <summary>
    /// Disables encryption for this session.
    /// </summary>
    public void DisableEncryption()
    {
        lock (_stateSync)
        {
            EncryptionEnabled = false;
        }
    }

    /// <summary>
    /// Enables compression for this session.
    /// </summary>
    public void EnableCompression()
    {
        lock (_stateSync)
        {
            CompressionEnabled = true;
        }
    }

    /// <summary>
    /// Enables encryption for this session.
    /// </summary>
    public void EnableEncryption()
    {
        lock (_stateSync)
        {
            EncryptionEnabled = true;
        }
    }

    /// <summary>
    /// Marks the session as in-game and stores selected character serial.
    /// </summary>
    /// <param name="characterSerial">Character serial.</param>
    public void EnterGame(uint characterSerial)
    {
        lock (_stateSync)
        {
            CharacterSerial = characterSerial;
            State = NetworkSessionState.InGame;
        }
    }

    /// <summary>
    /// Marks the session as authenticated and stores account metadata.
    /// </summary>
    /// <param name="accountId">Authenticated account id.</param>
    /// <param name="accountName">Authenticated account name.</param>
    public void MarkAuthenticated(long accountId, string accountName)
    {
        lock (_stateSync)
        {
            AccountId = accountId;
            AccountName = accountName;
            State = NetworkSessionState.Authenticated;
        }
    }

    /// <summary>
    /// Updates endpoint information from current client state.
    /// </summary>
    /// <param name="client">Source TCP client.</param>
    public void Refresh(MoongateTCPClient client)
    {
        _client = client;
        RemoteEndPoint = client.RemoteEndPoint?.ToString();
    }

    /// <summary>
    /// Updates the protocol state for this session.
    /// </summary>
    /// <param name="state">New session state.</param>
    public void SetState(NetworkSessionState state)
    {
        lock (_stateSync)
        {
            State = state;
        }
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
