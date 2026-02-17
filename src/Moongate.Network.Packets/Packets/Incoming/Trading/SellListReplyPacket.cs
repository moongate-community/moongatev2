using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Trading;

[PacketHandler(0x9F, PacketSizing.Variable, Description = "Sell List Reply")]
public class SellListReplyPacket : BaseGameNetworkPacket
{
    public SellListReplyPacket()
        : base(0x9F) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
