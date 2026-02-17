using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Login;

[PacketHandler(0xBD, PacketSizing.Variable, Description = "Client Version")]
public class ClientVersionPacket : BaseGameNetworkPacket
{
    public ClientVersionPacket()
        : base(0xBD, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
