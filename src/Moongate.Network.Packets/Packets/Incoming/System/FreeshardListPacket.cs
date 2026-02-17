using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.System;

[PacketHandler(0xF1, PacketSizing.Variable, Description = "Freeshard List")]
public class FreeshardListPacket : BaseGameNetworkPacket
{
    public FreeshardListPacket()
        : base(0xF1) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
