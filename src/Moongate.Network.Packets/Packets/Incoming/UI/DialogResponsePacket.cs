using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.UI;

[PacketHandler(0x7D, PacketSizing.Fixed, Length = 13, Description = "Response To Dialog Box")]
public class DialogResponsePacket : BaseGameNetworkPacket
{
    public DialogResponsePacket()
        : base(0x7D, 13) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
