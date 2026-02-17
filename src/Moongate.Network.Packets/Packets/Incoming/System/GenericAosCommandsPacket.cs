using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.System;

[PacketHandler(0xD7, PacketSizing.Variable, Description = "Generic AOS Commands")]
public class GenericAosCommandsPacket : BaseGameNetworkPacket
{
    public GenericAosCommandsPacket()
        : base(0xD7, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
