using System.Collections.Concurrent;
using Moongate.Network.Client;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Services;

namespace Moongate.Server.Services;

public sealed class GameNetworkSessionService : IGameNetworkSessionService
{
    private readonly ConcurrentDictionary<long, GameNetworkSession> _sessions = new();

    public GameNetworkSession GetOrCreate(MoongateTCPClient client)
    {
        var session = _sessions.GetOrAdd(client.SessionId, _ => new(client));
        session.Refresh(client);

        return session;
    }

    public bool TryGet(long sessionId, out GameNetworkSession session)
        => _sessions.TryGetValue(sessionId, out session!);

    public bool Remove(long sessionId)
        => _sessions.TryRemove(sessionId, out _);

    public void Clear()
        => _sessions.Clear();
}
