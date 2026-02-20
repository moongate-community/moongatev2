using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Types;

namespace Moongate.Network.Packets.Outgoing.World;

[PacketHandler(0x6D, PacketSizing.Fixed, Length = 3, Description = "Set Music")]
public class SetMusicPacket : BaseGameNetworkPacket
{
    public int MusicId { get; set; }

    public SetMusicPacket()
        : base(0x6D, 3) { }

    public SetMusicPacket(int musicId)
        : this()
        => MusicId = musicId;

    public SetMusicPacket(MusicName music)
        : this((int)music) { }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((ushort)MusicId);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 2)
        {
            return false;
        }

        MusicId = reader.ReadUInt16();

        return true;
    }
}
