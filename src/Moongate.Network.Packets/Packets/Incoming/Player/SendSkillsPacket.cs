using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Player;

[PacketHandler(0x3A, PacketSizing.Variable, Description = "Send Skills")]
public class SendSkillsPacket : BaseGameNetworkPacket
{
    public SendSkillsPacket()
        : base(0x3A, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
