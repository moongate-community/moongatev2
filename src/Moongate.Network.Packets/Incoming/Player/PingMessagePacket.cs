using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Incoming.Player;

[PacketHandler(0x73, PacketSizing.Fixed, Length = 2, Description = "Ping Message")]
public class PingMessagePacket : BaseGameNetworkPacket
{
    public byte Sequence { get; set; }

    public PingMessagePacket()
        : base(0x73, 2) { }

    public PingMessagePacket(byte sequence)
        : this()
        => Sequence = sequence;

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write(Sequence);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining < 1)
        {
            return false;
        }

        Sequence = reader.ReadByte();

        return reader.Remaining == 0;
    }
}
