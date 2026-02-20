using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Outgoing.World;

[PacketHandler(0x5B, PacketSizing.Fixed, Length = 4, Description = "Set Time")]
public class SetTimePacket : BaseGameNetworkPacket
{
    public DateTime Time { get; set; }

    public SetTimePacket()
        : base(0x5B, 4)
        => Time = DateTime.UtcNow;

    public SetTimePacket(DateTime time)
        : this()
        => Time = time;

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((byte)Time.Hour);
        writer.Write((byte)Time.Minute);
        writer.Write((byte)Time.Second);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 3)
        {
            return false;
        }

        var hour = reader.ReadByte();
        var minute = reader.ReadByte();
        var second = reader.ReadByte();
        Time = DateTime.UtcNow.Date.AddHours(hour).AddMinutes(minute).AddSeconds(second);

        return true;
    }
}
