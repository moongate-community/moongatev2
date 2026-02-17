using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Interaction;

[PacketHandler(0x07, PacketSizing.Fixed, Length = 7, Description = "Pick Up Item")]
public class PickUpPacket : BaseGameNetworkPacket
{
    public PickUpPacket()
        : base(0x07, 7) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
