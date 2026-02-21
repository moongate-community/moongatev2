using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Speech;

[PacketHandler(0xB5, PacketSizing.Fixed, Length = 64, Description = "Open Chat Window")]
public class OpenChatWindowPacket : BaseGameNetworkPacket
{
    public OpenChatWindowPacket()
        : base(0xB5, 64) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
