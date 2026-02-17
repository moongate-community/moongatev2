using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Map;

[PacketHandler(0x56, PacketSizing.Fixed, Length = 11)]
public class MapPacket : BaseGameNetworkPacket
{
    public MapPacket()
        : base(0x56, 11) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
