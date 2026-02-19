using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Trading;

[PacketHandler(0x6F, PacketSizing.Variable, Description = "Secure Trading")]
public class SecureTradingPacket : BaseGameNetworkPacket
{
    public SecureTradingPacket()
        : base(0x6F) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
