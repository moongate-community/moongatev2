using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.System;

[PacketHandler(0xBE, PacketSizing.Variable, Description = "Assist Version")]
public class AssistVersionPacket : BaseGameNetworkPacket
{
    public AssistVersionPacket()
        : base(0xBE) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
