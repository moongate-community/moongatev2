using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Movement;

[PacketHandler(0x72, PacketSizing.Variable)]
public class RequestWarModePacket : BaseGameNetworkPacket
{
    public RequestWarModePacket()
        : base(0x72, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
