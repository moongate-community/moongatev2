using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Outgoing.Login;

[PacketHandler(0xA9, PacketSizing.Variable, Description = "Characters / Starting Locations")]
public class CharacterListPacket : BaseGameNetworkPacket
{
    public CharacterListPacket()
        : base(0xA9) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
