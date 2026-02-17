using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Outgoing.Login;

[PacketHandler(0x1B, PacketSizing.Fixed, Length = 37, Description = "Char Locale and Body")]
public class LoginConfirmPacket : BaseGameNetworkPacket
{
    public LoginConfirmPacket()
        : base(0x1B, 37) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
