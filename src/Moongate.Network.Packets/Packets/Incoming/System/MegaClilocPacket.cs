using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.System;

[PacketHandler(0xD6, PacketSizing.Variable, Description = "Mega Cliloc")]
public class MegaClilocPacket : BaseGameNetworkPacket
{
    public MegaClilocPacket()
        : base(0xD6) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
