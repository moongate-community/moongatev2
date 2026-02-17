using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Login;

[PacketHandler(0x83, PacketSizing.Fixed, Length = 39)]
public class DeleteCharacterPacket : BaseGameNetworkPacket
{
    public DeleteCharacterPacket()
        : base(0x83, 39) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
