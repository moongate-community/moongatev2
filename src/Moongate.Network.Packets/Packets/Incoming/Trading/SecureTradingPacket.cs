using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Trading;

[PacketHandler(0x6F, PacketSizing.Variable)]
public class SecureTradingPacket : BaseGameNetworkPacket
{
    public SecureTradingPacket()
        : base(0x6F, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
