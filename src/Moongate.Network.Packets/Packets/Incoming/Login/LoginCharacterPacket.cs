using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Login;

[PacketHandler(0x5D, PacketSizing.Fixed, Length = 73, Description = "Login Character")]
public class LoginCharacterPacket : BaseGameNetworkPacket
{
    public LoginCharacterPacket()
        : base(0x5D, 73) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
