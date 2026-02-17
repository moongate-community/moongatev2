using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Outgoing.Movement;

[PacketHandler(0x22, PacketSizing.Fixed, Length = 3)]
public class MoveConfirmPacket : BaseGameNetworkPacket
{
    public MoveConfirmPacket()
        : base(0x22, 3) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
