using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.Network.Packets.Outgoing.Login;

[PacketHandler(0x1B, PacketSizing.Fixed, Length = 37, Description = "Char Locale and Body")]
public class LoginConfirmPacket : BaseGameNetworkPacket
{
    public UOMobileEntity? Mobile { get; set; }

    public LoginConfirmPacket()
        : base(0x1B, 37) { }

    public LoginConfirmPacket(UOMobileEntity mobile)
        : this()
        => Mobile = mobile;

    public override void Write(ref SpanWriter writer)
    {
        if (Mobile is null)
        {
            throw new InvalidOperationException("Mobile must be set before writing LoginConfirmPacket.");
        }

        writer.Write(OpCode);
        writer.Write(Mobile.Id.Value);
        writer.Write(0);
        writer.Write((short)Mobile.Body);
        writer.Write((short)Mobile.Location.X);
        writer.Write((short)Mobile.Location.Y);
        writer.Write((short)Mobile.Location.Z);
        writer.Write((byte)Mobile.Direction);
        writer.Write((byte)0);
        writer.Write(-1);
        writer.Write(0);

        var map = Mobile.Map;
        writer.Write((short)(map?.Width ?? 0));
        writer.Write((short)(map?.Height ?? 0));
        writer.Clear(37 - writer.Position);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 36)
        {
            return false;
        }

        return true;
    }
}
