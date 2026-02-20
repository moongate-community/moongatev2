using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Types;

namespace Moongate.Network.Packets.Outgoing.Movement;

[PacketHandler(0x97, PacketSizing.Fixed, Length = 2, Description = "Move Player")]
public class MovePlayerPacket : BaseGameNetworkPacket
{
    public DirectionType Direction { get; set; }

    public MovePlayerPacket()
        : base(0x97, 2) { }

    public MovePlayerPacket(DirectionType direction)
        : this()
        => Direction = direction;

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((byte)Direction);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 1)
        {
            return false;
        }

        Direction = (DirectionType)reader.ReadByte();

        return true;
    }
}
