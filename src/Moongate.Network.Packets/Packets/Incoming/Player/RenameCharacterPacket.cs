using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Player;

[PacketHandler(0x75, PacketSizing.Variable)]
public class RenameCharacterPacket : BaseGameNetworkPacket
{
    public RenameCharacterPacket()
        : base(0x75, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
