using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Outgoing.Movement;

[PacketHandler(0x22, PacketSizing.Fixed, Length = 3, Description = "Character Move ACK/ Resync Request")]
public class MoveConfirmPacket : BaseGameNetworkPacket
{
    public byte Notoriety { get; set; }

    public byte Sequence { get; set; }

    public MoveConfirmPacket()
        : base(0x22, 3) { }

    public MoveConfirmPacket(byte sequence, byte notoriety)
        : this()
    {
        Sequence = sequence;
        Notoriety = notoriety;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write(Sequence);
        writer.Write(Notoriety);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 2)
        {
            return false;
        }

        Sequence = reader.ReadByte();
        Notoriety = reader.ReadByte();

        return true;
    }
}
