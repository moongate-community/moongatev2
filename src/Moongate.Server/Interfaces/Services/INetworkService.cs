using Moongate.Abstractions.Interfaces.Services.Base;
using Moongate.Server.Data.Packets;
using Moongate.Server.Interfaces.Listener;

namespace Moongate.Server.Interfaces.Services;

public interface INetworkService : IMoongateService, IDisposable
{
    IReadOnlyCollection<GamePacket> ParsedPackets { get; }

    void AddPacketListener(byte OpCode, IPacketListener packetListener);

    bool TryDequeueParsedPacket(out GamePacket gamePacket);
}
