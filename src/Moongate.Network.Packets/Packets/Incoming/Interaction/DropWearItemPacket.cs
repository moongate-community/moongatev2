using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Interaction;

[PacketHandler(0x13, PacketSizing.Variable)]
public class DropWearItemPacket : BaseGameNetworkPacket
{
    public DropWearItemPacket()
        : base(0x13, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
