using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.System;

[PacketHandler(0xD9, PacketSizing.Variable, Description = "Spy On Client")]
public class SpyOnClientPacket : BaseGameNetworkPacket
{
    public SpyOnClientPacket()
        : base(0xD9, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
