using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.Network.Packets.Outgoing.Entity;

[PacketHandler(0x88, PacketSizing.Fixed, Length = 66, Description = "Paperdoll")]
public class PaperdollPacket : BaseGameNetworkPacket
{
    public UOMobileEntity? Mobile { get; set; }

    public PaperdollPacket()
        : base(0x88, 66) { }

    public PaperdollPacket(UOMobileEntity mobile)
        : this()
    {
        Mobile = mobile;
    }

    public override void Write(ref SpanWriter writer)
    {
        if (Mobile is null)
        {
            throw new InvalidOperationException("Mobile must be set before writing PaperdollPacket.");
        }

        writer.Write(OpCode);
        writer.Write(Mobile.Id.Value);

        var displayName = string.IsNullOrWhiteSpace(Mobile.Title)
                              ? Mobile.Name ?? string.Empty
                              : $"{Mobile.Name} {Mobile.Title}";

        writer.WriteAscii(displayName, 60);

        byte flags = 0x00;

        if (Mobile.IsWarMode)
        {
            flags |= 0x01;
        }

        // Allow lifting items from the paperdoll by default, matching baseline server behavior.
        flags |= 0x02;
        writer.Write(flags);
    }

    protected override bool ParsePayload(ref SpanReader reader)
        => reader.Remaining >= 2;
}
