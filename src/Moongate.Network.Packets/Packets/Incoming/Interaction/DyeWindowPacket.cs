using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Interaction;

[PacketHandler(0x95, PacketSizing.Fixed, Length = 9, Description = "Dye Window")]
public class DyeWindowPacket : BaseGameNetworkPacket
{
    public DyeWindowPacket()
        : base(0x95, 9) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
