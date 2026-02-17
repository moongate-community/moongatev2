using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Interaction;

[PacketHandler(0x95, PacketSizing.Variable)]
public class DyeWindowPacket : BaseGameNetworkPacket
{
    public DyeWindowPacket()
        : base(0x95, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
