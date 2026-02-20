using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.Network.Packets.Outgoing.Entity;

[PacketHandler(0x20, PacketSizing.Fixed, Length = 19, Description = "Draw Game Player")]
public class DrawPlayerPacket : BaseGameNetworkPacket
{
    public UOMobileEntity? Mobile { get; set; }

    public DrawPlayerPacket()
        : base(0x20, 19) { }

    public DrawPlayerPacket(UOMobileEntity mobile)
        : this()
        => Mobile = mobile;

    public override void Write(ref SpanWriter writer)
    {
        if (Mobile is null)
        {
            throw new InvalidOperationException("Mobile must be set before writing DrawPlayerPacket.");
        }

        writer.Write(OpCode);
        writer.Write(Mobile.Id.Value);
        writer.Write((short)Mobile.Body);
        writer.Write((byte)0);
        writer.Write((ushort)Mobile.SkinHue);
        writer.Write(Mobile.GetPacketFlags(stygianAbyss: true));
        writer.Write((ushort)Mobile.Location.X);
        writer.Write((ushort)Mobile.Location.Y);
        writer.Write((ushort)0);
        writer.Write((byte)Mobile.Direction);
        writer.Write((sbyte)Mobile.Location.Z);
    }

    protected override bool ParsePayload(ref SpanReader reader)
        => reader.Remaining == 18;
}
