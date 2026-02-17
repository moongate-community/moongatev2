using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Interaction;

[PacketHandler(0x09, PacketSizing.Fixed, Length = 5)]
public class SingleClickPacket : BaseGameNetworkPacket
{
    public SingleClickPacket()
        : base(0x09, 5) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
