using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Types;

namespace Moongate.Network.Packets.Outgoing.Movement;

[PacketHandler(0x21, PacketSizing.Fixed, Length = 8, Description = "Char Move Rejection")]
public class MoveDenyPacket : BaseGameNetworkPacket
{
    public DirectionType Direction { get; set; }

    public byte Sequence { get; set; }

    public short X { get; set; }

    public short Y { get; set; }

    public sbyte Z { get; set; }

    public MoveDenyPacket()
        : base(0x21, 8) { }

    public MoveDenyPacket(byte sequence, short x, short y, DirectionType direction, sbyte z)
        : this()
    {
        Sequence = sequence;
        X = x;
        Y = y;
        Direction = direction;
        Z = z;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write(Sequence);
        writer.Write(X);
        writer.Write(Y);
        writer.Write((byte)Direction);
        writer.Write(Z);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 7)
        {
            return false;
        }

        Sequence = reader.ReadByte();
        X = reader.ReadInt16();
        Y = reader.ReadInt16();
        Direction = (DirectionType)reader.ReadByte();
        Z = reader.ReadSByte();

        return true;
    }
}
