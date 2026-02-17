using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Player;

[PacketHandler(0x34, PacketSizing.Fixed, Length = 10, Description = "Get Player Status")]
public class GetPlayerStatusPacket : BaseGameNetworkPacket
{
    public GetPlayerStatusPacket()
        : base(0x34, 10) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
