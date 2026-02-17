using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Targeting;

[PacketHandler(0x6C, PacketSizing.Fixed, Length = 19)]
public class TargetCursorCommandsPacket : BaseGameNetworkPacket
{
    public TargetCursorCommandsPacket()
        : base(0x6C, 19) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
