using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Outgoing.Login;

[PacketHandler(0xA8, PacketSizing.Variable)]
public class ServerListPacket : BaseGameNetworkPacket
{
    public ServerListPacket()
        : base(0xA8) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
