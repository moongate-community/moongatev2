using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Speech;

[PacketHandler(0xB3, PacketSizing.Variable)]
public class ChatTextPacket : BaseGameNetworkPacket
{
    public ChatTextPacket()
        : base(0xB3, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
