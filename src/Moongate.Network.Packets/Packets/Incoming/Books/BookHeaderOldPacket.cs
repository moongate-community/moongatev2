using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Books;

[PacketHandler(0x93, PacketSizing.Variable)]
public class BookHeaderOldPacket : BaseGameNetworkPacket
{
    public BookHeaderOldPacket()
        : base(0x93, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
