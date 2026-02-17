using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Login;

[PacketHandler(0xA0, PacketSizing.Fixed, Length = 3, Description = "Select Server")]
public class ServerSelectPacket : BaseGameNetworkPacket
{
    public ServerSelectPacket()
        : base(0xA0, 3) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
