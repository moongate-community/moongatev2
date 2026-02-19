using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Speech;

[PacketHandler(0xC2, PacketSizing.Variable, Description = "Unicode TextEntry")]
public class UnicodeTextEntryPacket : BaseGameNetworkPacket
{
    public UnicodeTextEntryPacket()
        : base(0xC2) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
