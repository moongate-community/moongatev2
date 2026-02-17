using System.Collections.Concurrent;
using Moongate.Network.Client;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Services;

namespace Moongate.Server.Services;

public sealed class GameNetworkSessionService : IGameNetworkSessionService
{
    private readonly ConcurrentDictionary<long, GameSession> _sessions = new();

    public void Clear()
        => _sessions.Clear();

    public GameSession GetOrCreate(MoongateTCPClient client)
    {
        var session = _sessions.GetOrAdd(client.SessionId, _ => new(new GameNetworkSession(client)));
        session.NetworkSession.Refresh(client);

        return session;
    }

    public bool Remove(long sessionId)
        => _sessions.TryRemove(sessionId, out _);

    public bool TryGet(long sessionId, out GameSession session)
        => _sessions.TryGetValue(sessionId, out session!);
}
