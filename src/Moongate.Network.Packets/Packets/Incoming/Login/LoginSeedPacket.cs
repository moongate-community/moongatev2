using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Login;

[PacketHandler(0xEF, PacketSizing.Fixed, Length = 21, Description = "KR/2D Client Login/Seed")]
public class LoginSeedPacket : BaseGameNetworkPacket
{
    public LoginSeedPacket()
        : base(0xEF, 21) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
