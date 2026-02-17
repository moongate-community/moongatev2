using Moongate.Server.Interfaces.Listener;

namespace Moongate.Server.Interfaces.Services;

public interface INetworkService
{
    void AddPacketListener(byte OpCode, IPacketListener packetListener);
}
