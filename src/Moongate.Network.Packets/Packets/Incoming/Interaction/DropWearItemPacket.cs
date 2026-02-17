using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Interaction;

[PacketHandler(0x13, PacketSizing.Fixed, Length = 10, Description = "Drop->Wear Item")]
public class DropWearItemPacket : BaseGameNetworkPacket
{
    public DropWearItemPacket()
        : base(0x13, 10) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
