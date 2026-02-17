using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Player;

[PacketHandler(0x01, PacketSizing.Variable)]
public class DisconnectNotificationPacket : BaseGameNetworkPacket
{
    public DisconnectNotificationPacket()
        : base(0x01, -1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}
