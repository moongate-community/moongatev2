using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.UI;

[PacketHandler(0xA7, PacketSizing.Fixed, Length = 4, Description = "Request Tip/Notice Window")]
public class RequestTipNoticeWindowPacket : BaseGameNetworkPacket
{
    public RequestTipNoticeWindowPacket()
        : base(0xA7, 4) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
