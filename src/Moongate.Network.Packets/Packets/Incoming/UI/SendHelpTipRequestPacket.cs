using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.UI;

[PacketHandler(0xB6, PacketSizing.Variable)]
public class SendHelpTipRequestPacket : BaseGameNetworkPacket
{
    public SendHelpTipRequestPacket()
        : base(0xB6, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
