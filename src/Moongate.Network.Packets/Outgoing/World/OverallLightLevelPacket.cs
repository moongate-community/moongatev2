using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Types;

namespace Moongate.Network.Packets.Outgoing.World;

[PacketHandler(0x4F, PacketSizing.Fixed, Length = 2, Description = "Overall Light Level")]
public class OverallLightLevelPacket : BaseGameNetworkPacket
{
    public LightLevelType LightLevel { get; set; }

    public OverallLightLevelPacket()
        : base(0x4F, 2) { }

    public OverallLightLevelPacket(LightLevelType lightLevel)
        : this()
        => LightLevel = lightLevel;

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((byte)LightLevel);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 1)
        {
            return false;
        }

        LightLevel = (LightLevelType)reader.ReadByte();

        return true;
    }
}
