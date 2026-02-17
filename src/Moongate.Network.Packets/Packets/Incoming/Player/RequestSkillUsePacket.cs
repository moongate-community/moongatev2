using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Player;

[PacketHandler(0x12, PacketSizing.Variable, Description = "Request Skill etc use")]
public class RequestSkillUsePacket : BaseGameNetworkPacket
{
    public RequestSkillUsePacket()
        : base(0x12) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
