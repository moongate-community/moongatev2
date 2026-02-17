using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.UI;

[PacketHandler(0xB6, PacketSizing.Fixed, Length = 9)]
public class SendHelpTipRequestPacket : BaseGameNetworkPacket
{
    public SendHelpTipRequestPacket()
        : base(0xB6, 9) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
