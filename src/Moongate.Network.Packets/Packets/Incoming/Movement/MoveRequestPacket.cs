using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Movement;

[PacketHandler(0x02, PacketSizing.Fixed, Length = 7)]
public class MoveRequestPacket : BaseGameNetworkPacket
{
    public MoveRequestPacket()
        : base(0x02, 7) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
