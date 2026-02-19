using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Player;

[PacketHandler(0x98, PacketSizing.Variable, Description = "All Names (3D Client Only)")]
public class AllNamesPacket : BaseGameNetworkPacket
{
    public AllNamesPacket()
        : base(0x98) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
