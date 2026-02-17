using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Login;

[PacketHandler(0x83, PacketSizing.Variable)]
public class DeleteCharacterPacket : BaseGameNetworkPacket
{
    public DeleteCharacterPacket()
        : base(0x83, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
