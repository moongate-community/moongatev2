using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Speech;

[PacketHandler(0xC2, PacketSizing.Variable)]
public class UnicodeTextEntryPacket : BaseGameNetworkPacket
{
    public UnicodeTextEntryPacket()
        : base(0xC2, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
