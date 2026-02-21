using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Types;
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
    ///  Gets or sets the currently active character identifier for this session, when a character is selected.
    /// </summary>
    public Serial CharacterId { get; set; }

    /// <summary>
    /// Gets or sets the runtime mobile entity bound to this session.
    /// </summary>
    public UOMobileEntity? Character { get; set; }

    /// <summary>
    /// Gets or sets the current ping sequence number for this session, used for latency monitoring and connection health checks.
    /// </summary>
    public byte PingSequence { get; set; }

    /// <summary>
    /// Gets or sets the movement sequence state used to validate move requests.
    /// </summary>
    public byte MoveSequence { get; set; }

    /// <summary>
    /// Gets or sets the cached self-notoriety byte used by movement acknowledgements.
    /// </summary>
    public byte SelfNotoriety { get; set; } = 0x01;

    /// <summary>
    /// Gets or sets the next eligible movement tick timestamp in milliseconds.
    /// </summary>
    public long MoveTime { get; set; }

    /// <summary>
    /// Gets or sets movement throttle credit in milliseconds.
    /// </summary>
    public long MoveCredit { get; set; }

    /// <summary>
    /// Gets or sets whether the controlled character is currently mounted.
    /// </summary>
    public bool IsMounted { get; set; }

    /// <summary>
    /// Stores the negotiated client version for this session.
    /// </summary>
    /// <param name="clientVersion">Client version metadata.</param>
    public void SetClientVersion(ClientVersion clientVersion)
        => ClientVersion = clientVersion;
}
