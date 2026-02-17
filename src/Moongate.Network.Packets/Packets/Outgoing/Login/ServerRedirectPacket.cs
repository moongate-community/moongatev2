using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Outgoing.Login;

[PacketHandler(0x8C, PacketSizing.Fixed, Length = 11)]
public class ServerRedirectPacket : BaseGameNetworkPacket
{
    public ServerRedirectPacket()
        : base(0x8C, 11) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
