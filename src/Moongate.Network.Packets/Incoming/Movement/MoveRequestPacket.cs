using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Types;

namespace Moongate.Network.Packets.Incoming.Movement;

[PacketHandler(0x02, PacketSizing.Fixed, Length = 7, Description = "Move Request")]
public class MoveRequestPacket : BaseGameNetworkPacket
{
    public DirectionType Direction { get; set; }

    public DirectionType WalkDirection => (DirectionType)((byte)Direction & 0x07);

    public bool IsRunning => (Direction & DirectionType.Running) != 0;

    public byte Sequence { get; set; }

    public uint FastWalkKey { get; set; }

    public MoveRequestPacket()
        : base(0x02, 7) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 6)
        {
            return false;
        }

        Direction = (DirectionType)reader.ReadByte();
        Sequence = reader.ReadByte();
        FastWalkKey = reader.ReadUInt32();

        return true;
    }
}
