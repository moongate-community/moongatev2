using Moongate.Network.Packets.Interfaces;
using Moongate.Server.Data.Session;

namespace Moongate.Server.Interfaces.Listener;

public interface IPacketListener
{
    Task<bool> HandlePacketAsync(GameNetworkSession session, IGameNetworkPacket packet);
}
