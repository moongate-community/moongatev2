using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Player;

[PacketHandler(0x73, PacketSizing.Variable)]
public class PingMessagePacket : BaseGameNetworkPacket
{
    public PingMessagePacket()
        : base(0x73, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
