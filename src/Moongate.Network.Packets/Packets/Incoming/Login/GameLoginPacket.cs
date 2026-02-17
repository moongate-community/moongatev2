using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Login;

[PacketHandler(0x91, PacketSizing.Fixed, Length = 65)]
public class GameLoginPacket : BaseGameNetworkPacket
{
    public GameLoginPacket()
        : base(0x91, 65) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
