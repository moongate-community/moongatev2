using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Interaction;

[PacketHandler(0x71, PacketSizing.Variable, Description = "Bulletin Board Messages")]
public class BulletinBoardMessagesPacket : BaseGameNetworkPacket
{
    public BulletinBoardMessagesPacket()
        : base(0x71, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
