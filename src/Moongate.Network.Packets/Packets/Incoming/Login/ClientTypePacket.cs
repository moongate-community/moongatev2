using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Login;

[PacketHandler(0xE1, PacketSizing.Variable)]
public class ClientTypePacket : BaseGameNetworkPacket
{
    public ClientTypePacket()
        : base(0xE1, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
