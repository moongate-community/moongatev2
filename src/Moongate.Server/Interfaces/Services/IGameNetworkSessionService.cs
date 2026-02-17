using Moongate.Network.Client;
using Moongate.Server.Data.Session;

namespace Moongate.Server.Interfaces.Services;

/// <summary>
/// Maintains per-client game network sessions.
/// </summary>
public interface IGameNetworkSessionService
{
    /// <summary>
    /// Clears all sessions.
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets an existing session or creates one for the specified client.
    /// </summary>
    /// <param name="client">Client connection.</param>
    /// <returns>Session state object.</returns>
    GameSession GetOrCreate(MoongateTCPClient client);

    /// <summary>
    /// Removes a session from the store.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <returns><c>true</c> when removed.</returns>
    bool Remove(long sessionId);

    /// <summary>
    /// Tries to get a session by identifier.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <param name="session">Resolved session.</param>
    /// <returns><c>true</c> when session exists.</returns>
    bool TryGet(long sessionId, out GameSession session);
}
