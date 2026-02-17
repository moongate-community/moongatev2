using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.UI;

[PacketHandler(0x2C, PacketSizing.Variable)]
public class ResurrectionMenuPacket : BaseGameNetworkPacket
{
    public ResurrectionMenuPacket()
        : base(0x2C, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
