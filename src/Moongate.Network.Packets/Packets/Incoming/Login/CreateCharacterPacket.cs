using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Login;

[PacketHandler(0x00, PacketSizing.Fixed, Length = 104)]
public class CreateCharacterPacket : BaseGameNetworkPacket
{
    public CreateCharacterPacket()
        : base(0x00, 104) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
