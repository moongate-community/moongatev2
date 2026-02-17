using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Player;

[PacketHandler(0xB8, PacketSizing.Variable)]
public class RequestCharProfilePacket : BaseGameNetworkPacket
{
    public RequestCharProfilePacket()
        : base(0xB8, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
