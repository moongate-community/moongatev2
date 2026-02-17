using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.UI;

[PacketHandler(0x9B, PacketSizing.Fixed, Length = 258)]
public class RequestHelpPacket : BaseGameNetworkPacket
{
    public RequestHelpPacket()
        : base(0x9B, 258) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
