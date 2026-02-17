using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Interaction;

[PacketHandler(0x05, PacketSizing.Fixed, Length = 5, Description = "Request Attack")]
public class RequestAttackPacket : BaseGameNetworkPacket
{
    public RequestAttackPacket()
        : base(0x05, 5) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
