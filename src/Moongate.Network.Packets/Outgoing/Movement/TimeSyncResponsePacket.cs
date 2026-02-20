using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Outgoing.Movement;

[PacketHandler(0xF2, PacketSizing.Fixed, Length = 13, Description = "Time Sync Response")]
public class TimeSyncResponsePacket : BaseGameNetworkPacket
{
    public int Tick1 { get; set; }

    public int Tick2 { get; set; }

    public int Tick3 { get; set; }

    public TimeSyncResponsePacket()
        : base(0xF2, 13)
    {
        var tick = Environment.TickCount;
        Tick1 = tick;
        Tick2 = tick;
        Tick3 = tick;
    }

    public TimeSyncResponsePacket(int tick1, int tick2, int tick3)
        : this()
    {
        Tick1 = tick1;
        Tick2 = tick2;
        Tick3 = tick3;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write(Tick1);
        writer.Write(Tick2);
        writer.Write(Tick3);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 12)
        {
            return false;
        }

        Tick1 = reader.ReadInt32();
        Tick2 = reader.ReadInt32();
        Tick3 = reader.ReadInt32();

        return true;
    }
}
