using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Login;

[PacketHandler(0xF8, PacketSizing.Fixed, Length = 106, Description = "Character Creation ( 7.0.16.0 )")]
public class CharacterCreation70160Packet : BaseGameNetworkPacket
{
    public CharacterCreation70160Packet()
        : base(0xF8, 106) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
