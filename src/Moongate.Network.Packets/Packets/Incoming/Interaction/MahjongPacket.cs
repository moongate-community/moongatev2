using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Interaction;

[PacketHandler(0xDA, PacketSizing.Variable)]
public class MahjongPacket : BaseGameNetworkPacket
{
    public MahjongPacket()
        : base(0xDA, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
