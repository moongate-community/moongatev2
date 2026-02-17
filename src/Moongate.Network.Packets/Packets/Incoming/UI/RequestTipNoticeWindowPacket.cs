using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.UI;

[PacketHandler(0xA7, PacketSizing.Variable)]
public class RequestTipNoticeWindowPacket : BaseGameNetworkPacket
{
    public RequestTipNoticeWindowPacket()
        : base(0xA7, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
