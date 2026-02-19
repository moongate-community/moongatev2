using Moongate.Network.Packets.Interfaces;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Listener;

namespace Moongate.Tests.Server.Support;

public sealed class GameLoopThrowingPacketListener : IPacketListener
{
    public Task<bool> HandlePacketAsync(GameSession session, IGameNetworkPacket packet)
        => throw new InvalidOperationException("listener failure");
}
