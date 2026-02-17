using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Outgoing.Entity;

[PacketHandler(0x20, PacketSizing.Variable)]
public class DrawPlayerPacket : BaseGameNetworkPacket
{
    public DrawPlayerPacket()
        : base(0x20) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
