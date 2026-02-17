using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Trading;

[PacketHandler(0x9F, PacketSizing.Variable)]
public class SellListReplyPacket : BaseGameNetworkPacket
{
    public SellListReplyPacket()
        : base(0x9F, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
