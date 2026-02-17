using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.UI;

[PacketHandler(0x7D, PacketSizing.Variable)]
public class DialogResponsePacket : BaseGameNetworkPacket
{
    public DialogResponsePacket()
        : base(0x7D, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
