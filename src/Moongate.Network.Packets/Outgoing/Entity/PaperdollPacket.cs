using System.Buffers.Binary;
using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.Network.Packets.Outgoing.Entity;

[PacketHandler(0x88, PacketSizing.Variable, Description = "Paperdoll")]
public class PaperdollPacket : BaseGameNetworkPacket
{
    public UOMobileEntity? Mobile { get; set; }

    public string Title { get; set; } = string.Empty;

    public PaperdollPacket()
        : base(0x88) { }

    public PaperdollPacket(UOMobileEntity mobile, string? title = null)
        : this()
    {
        Mobile = mobile;
        Title = title ?? string.Empty;
    }

    public override void Write(ref SpanWriter writer)
    {
        if (Mobile is null)
        {
            throw new InvalidOperationException("Mobile must be set before writing PaperdollPacket.");
        }

        var start = writer.Position;

        writer.Write(OpCode);
        writer.Write((ushort)0);
        writer.Write(Mobile.Id.Value);

        var displayName = string.IsNullOrWhiteSpace(Title)
                              ? Mobile.Name ?? string.Empty
                              : $"{Mobile.Name} {Title}";

        writer.WriteAscii(displayName, 60);

        var flags = Mobile.GetPacketFlags(true);

        if (Mobile.IsWarMode)
        {
            flags |= 0x40;
        }

        flags |= 0x02;
        writer.Write(flags);

        var packetLength = (ushort)(writer.Position - start);
        BinaryPrimitives.WriteUInt16BigEndian(writer.RawBuffer[(start + 1)..], packetLength);
    }

    protected override bool ParsePayload(ref SpanReader reader)
        => reader.Remaining >= 2;
}
