using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Interaction;

[PacketHandler(0x08, PacketSizing.Fixed, Length = 14)]
public class DropPacket : BaseGameNetworkPacket
{
    public DropPacket()
        : base(0x08, 14) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
