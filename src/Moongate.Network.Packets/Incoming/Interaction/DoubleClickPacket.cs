using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Interaction;

[PacketHandler(0x06, PacketSizing.Fixed, Length = 5, Description = "Double Click")]
public class DoubleClickPacket : BaseGameNetworkPacket
{
    public DoubleClickPacket()
        : base(0x06, 5) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
