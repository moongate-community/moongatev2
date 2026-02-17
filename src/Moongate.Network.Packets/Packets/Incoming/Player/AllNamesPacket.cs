using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Player;

[PacketHandler(0x98, PacketSizing.Variable)]
public class AllNamesPacket : BaseGameNetworkPacket
{
    public AllNamesPacket()
        : base(0x98, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
