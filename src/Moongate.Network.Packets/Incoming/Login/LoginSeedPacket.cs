using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Version;

namespace Moongate.Network.Packets.Incoming.Login;

[PacketHandler(0xEF, PacketSizing.Fixed, Length = 21, Description = "KR/2D Client Login/Seed")]
public class LoginSeedPacket : BaseGameNetworkPacket
{
    public int Seed { get; set; }
    public ClientVersion ClientVersion { get; set; }

    public LoginSeedPacket()
        : base(0xEF, 21) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        Seed = reader.ReadInt32();
        ClientVersion = new(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

        return true;
    }
}
