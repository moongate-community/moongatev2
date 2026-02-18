using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Types;

namespace Moongate.Network.Packets.Outgoing.Login;


[PacketHandler(0x82, PacketSizing.Fixed, Length = 2, Description = "Login Denied Response")]
public class LoginDeniedPacket : BaseGameNetworkPacket
{
    public byte Reason { get; set; }

    public LoginDeniedPacket() : base(0x82, 2)
    {
    }

    public LoginDeniedPacket(UOLoginDeniedReason reason) : this()
    {
        Reason = (byte)reason;
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write(Reason);
    }
}
