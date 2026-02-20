using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Types;

namespace Moongate.Network.Packets.Outgoing.World;

[PacketHandler(0xBC, PacketSizing.Fixed, Length = 3, Description = "Season")]
public class SeasonPacket : BaseGameNetworkPacket
{
    public bool PlaySound { get; set; }

    public SeasonType Season { get; set; }

    public SeasonPacket()
        : base(0xBC, 3) { }

    public SeasonPacket(SeasonType season, bool playSound = true)
        : this()
    {
        Season = season;
        PlaySound = playSound;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((byte)Season);
        writer.Write(PlaySound);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 2)
        {
            return false;
        }

        Season = (SeasonType)reader.ReadByte();
        PlaySound = reader.ReadBoolean();

        return true;
    }
}
