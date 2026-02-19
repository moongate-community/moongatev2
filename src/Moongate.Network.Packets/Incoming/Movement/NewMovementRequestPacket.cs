using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Movement;

[PacketHandler(0xF0, PacketSizing.Variable, Description = "Krrios client special")]
public class NewMovementRequestPacket : BaseGameNetworkPacket
{
    public NewMovementRequestPacket()
        : base(0xF0) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
