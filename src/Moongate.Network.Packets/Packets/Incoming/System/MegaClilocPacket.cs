using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.System;

[PacketHandler(0xD6, PacketSizing.Variable)]
public class MegaClilocPacket : BaseGameNetworkPacket
{
    public MegaClilocPacket()
        : base(0xD6, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
