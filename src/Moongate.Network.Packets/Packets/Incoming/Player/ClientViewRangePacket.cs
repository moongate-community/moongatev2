using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Player;

[PacketHandler(0xC8, PacketSizing.Variable)]
public class ClientViewRangePacket : BaseGameNetworkPacket
{
    public ClientViewRangePacket()
        : base(0xC8, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
