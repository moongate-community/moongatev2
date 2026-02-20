using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Types;

namespace Moongate.Network.Packets.Outgoing.World;

[PacketHandler(0x4E, PacketSizing.Fixed, Length = 6, Description = "Personal Light Level")]
public class PersonalLightLevelPacket : BaseGameNetworkPacket
{
    public LightLevelType LightLevel { get; set; }

    public UOMobileEntity? Mobile { get; set; }

    public PersonalLightLevelPacket()
        : base(0x4E, 6) { }

    public PersonalLightLevelPacket(LightLevelType lightLevel, UOMobileEntity mobile)
        : this()
    {
        LightLevel = lightLevel;
        Mobile = mobile;
    }

    public override void Write(ref SpanWriter writer)
    {
        if (Mobile is null)
        {
            throw new InvalidOperationException("Mobile must be set before writing PersonalLightLevelPacket.");
        }

        writer.Write(OpCode);
        writer.Write(Mobile.Id.Value);
        writer.Write((byte)LightLevel);
    }

    protected override bool ParsePayload(ref SpanReader reader)
        => reader.Remaining == 5;
}
