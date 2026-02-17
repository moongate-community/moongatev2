using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Books;

[PacketHandler(0x66, PacketSizing.Variable, Description = "Books (Pages)")]
public class BookPagesPacket : BaseGameNetworkPacket
{
    public BookPagesPacket()
        : base(0x66, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
