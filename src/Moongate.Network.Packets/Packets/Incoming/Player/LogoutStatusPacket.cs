using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Player;

[PacketHandler(0xD1, PacketSizing.Fixed, Length = 2)]
public class LogoutStatusPacket : BaseGameNetworkPacket
{
    public LogoutStatusPacket()
        : base(0xD1, 2) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
