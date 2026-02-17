using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Outgoing.Entity;

[PacketHandler(0x78, PacketSizing.Variable, Description = "Draw Object")]
public class MobileIncomingPacket : BaseGameNetworkPacket
{
    public MobileIncomingPacket()
        : base(0x78) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
