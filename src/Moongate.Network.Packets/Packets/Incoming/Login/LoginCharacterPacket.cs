using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Login;

[PacketHandler(0x5D, PacketSizing.Variable)]
public class LoginCharacterPacket : BaseGameNetworkPacket
{
    public LoginCharacterPacket()
        : base(0x5D, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
