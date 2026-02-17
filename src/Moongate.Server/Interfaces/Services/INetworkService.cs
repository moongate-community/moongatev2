using Moongate.Abstractions.Interfaces.Services.Base;
using Moongate.Server.Interfaces.Listener;

namespace Moongate.Server.Interfaces.Services;

public interface INetworkService : IMoongateService, IDisposable
{
    void AddPacketListener(byte OpCode, IPacketListener packetListener);
}
