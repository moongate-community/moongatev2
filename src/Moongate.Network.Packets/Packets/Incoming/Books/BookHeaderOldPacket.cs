using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Books;

[PacketHandler(0x93, PacketSizing.Fixed, Length = 99, Description = "Book Header ( Old )")]
public class BookHeaderOldPacket : BaseGameNetworkPacket
{
    public BookHeaderOldPacket()
        : base(0x93, 99) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
