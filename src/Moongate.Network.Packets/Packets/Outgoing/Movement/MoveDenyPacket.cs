using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Outgoing.Movement;

[PacketHandler(0x21, PacketSizing.Fixed, Length = 8)]
public class MoveDenyPacket : BaseGameNetworkPacket
{
    public MoveDenyPacket()
        : base(0x21, 8) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
