using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.House;

[PacketHandler(0xFB, PacketSizing.Variable)]
public class UpdateViewPublicHouseContentsPacket : BaseGameNetworkPacket
{
    public UpdateViewPublicHouseContentsPacket()
        : base(0xFB, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
