using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Movement;

[PacketHandler(0x72, PacketSizing.Fixed, Length = 5, Description = "Request War Mode")]
public class RequestWarModePacket : BaseGameNetworkPacket
{
    public RequestWarModePacket()
        : base(0x72, 5) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
