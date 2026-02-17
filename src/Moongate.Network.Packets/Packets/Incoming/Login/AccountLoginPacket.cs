using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Login;

[PacketHandler(0x80, PacketSizing.Fixed, Length = 62)]
public class AccountLoginPacket : BaseGameNetworkPacket
{
    public AccountLoginPacket()
        : base(0x80, 62) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
