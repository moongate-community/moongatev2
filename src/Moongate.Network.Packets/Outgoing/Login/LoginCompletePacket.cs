using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Outgoing.Login;

[PacketHandler(0x55, PacketSizing.Fixed, Length = 1, Description = "Login Complete")]
public class LoginCompletePacket : BaseGameNetworkPacket
{
    public LoginCompletePacket()
        : base(0x55, 1) { }

    public override void Write(ref SpanWriter writer)
        => writer.Write(OpCode);

    protected override bool ParsePayload(ref SpanReader reader)
        => reader.Remaining == 0;
}
