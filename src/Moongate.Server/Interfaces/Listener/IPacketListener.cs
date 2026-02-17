using Moongate.Network.Packets.Interfaces;

namespace Moongate.Server.Interfaces.Listener;

public interface IPacketListener
{
    Task<bool> HandlePacketAsync(IGameNetworkPacket packet);
}
