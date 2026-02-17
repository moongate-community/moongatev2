using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Speech;

[PacketHandler(0xAD, PacketSizing.Variable, Description = "Unicode/Ascii speech request")]
public class UnicodeSpeechPacket : BaseGameNetworkPacket
{
    public UnicodeSpeechPacket()
        : base(0xAD) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
