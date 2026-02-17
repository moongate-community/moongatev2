using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Trading;

[PacketHandler(0x3B, PacketSizing.Variable)]
public class BuyItemsPacket : BaseGameNetworkPacket
{
    public BuyItemsPacket()
        : base(0x3B, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
