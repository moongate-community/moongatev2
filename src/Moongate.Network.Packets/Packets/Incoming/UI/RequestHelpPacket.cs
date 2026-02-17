using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.UI;

[PacketHandler(0x9B, PacketSizing.Variable)]
public class RequestHelpPacket : BaseGameNetworkPacket
{
    public RequestHelpPacket()
        : base(0x9B, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
