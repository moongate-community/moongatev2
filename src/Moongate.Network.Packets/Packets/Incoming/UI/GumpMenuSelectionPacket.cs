using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.UI;

[PacketHandler(0xB1, PacketSizing.Variable)]
public class GumpMenuSelectionPacket : BaseGameNetworkPacket
{
    public GumpMenuSelectionPacket()
        : base(0xB1, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
