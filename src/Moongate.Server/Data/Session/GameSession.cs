using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Version;

namespace Moongate.Server.Data.Session;

/// <summary>
/// Represents gameplay and protocol state for a connected client.
/// </summary>
public sealed class GameSession
{
    public GameSession(GameNetworkSession networkSession)
        => NetworkSession = networkSession;

    /// <summary>
    /// Gets the underlying transport session.
    /// </summary>
    public GameNetworkSession NetworkSession { get; }

    /// <summary>
    /// Gets the unique session identifier.
    /// </summary>
    public long SessionId => NetworkSession.SessionId;

    /// <summary>
    /// Gets the negotiated client version, when known.
    /// </summary>
    public ClientVersion? ClientVersion { get; private set; }

    /// <summary>
    /// Gets or sets the account identifier associated with this session, when authenticated.
    /// </summary>
    public Serial AccountId { get; set; }

    /// <summary>
    /// Gets or sets the current ping sequence number for this session, used for latency monitoring and connection health checks.
    /// </summary>
    public byte PingSequence { get; set; }

    /// <summary>
    /// Stores the negotiated client version for this session.
    /// </summary>
    /// <param name="clientVersion">Client version metadata.</param>
    public void SetClientVersion(ClientVersion clientVersion)
        => ClientVersion = clientVersion;
}
