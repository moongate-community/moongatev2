using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Books;

[PacketHandler(0xD4, PacketSizing.Variable)]
public class BookHeaderNewPacket : BaseGameNetworkPacket
{
    public BookHeaderNewPacket()
        : base(0xD4, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
