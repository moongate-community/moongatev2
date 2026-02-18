using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Login;

[PacketHandler(0x80, PacketSizing.Fixed, Length = 62, Description = "Login Request")]
public class AccountLoginPacket : BaseGameNetworkPacket
{
    public string Account { get; set; }
    public string Password { get; set; }

    public byte NextLoginKey { get; set; }

    public AccountLoginPacket()
        : base(0x80, 62) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        Account = reader.ReadAscii(30);
        Password = reader.ReadAscii(30);
        NextLoginKey = reader.ReadByte();


        return true;
    }
}
