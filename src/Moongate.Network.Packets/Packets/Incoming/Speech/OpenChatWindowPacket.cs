using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Speech;

[PacketHandler(0xB5, PacketSizing.Variable)]
public class OpenChatWindowPacket : BaseGameNetworkPacket
{
    public OpenChatWindowPacket()
        : base(0xB5, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
