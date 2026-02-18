using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Login;

[PacketHandler(0x91, PacketSizing.Fixed, Length = 65, Description = "Game Server Login")]
public class GameLoginPacket : BaseGameNetworkPacket
{
    public uint SessionKey { get; set; }
    public string AccountName { get; set; }
    public string Password { get; set; }

    public GameLoginPacket()
        : base(0x91, 65) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        SessionKey = reader.ReadUInt32();
        AccountName = reader.ReadAscii(30);
        Password = reader.ReadAscii(30);

        return true;
    }
}
