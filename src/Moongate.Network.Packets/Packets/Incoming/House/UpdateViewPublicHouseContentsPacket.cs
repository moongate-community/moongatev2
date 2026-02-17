using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.House;

[PacketHandler(0xFB, PacketSizing.Fixed, Length = 2)]
public class UpdateViewPublicHouseContentsPacket : BaseGameNetworkPacket
{
    public UpdateViewPublicHouseContentsPacket()
        : base(0xFB, 2) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
