using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Packets.Data;

namespace Moongate.Network.Packets.Outgoing.Login;

[PacketHandler(0xA8, PacketSizing.Variable, Description = "Game Server List")]
public class ServerListPacket : BaseGameNetworkPacket
{
    public List<GameServerEntry> Shards { get; } = [];

    public ServerListPacket()
        : base(0xA8) { }

    public ServerListPacket(params GameServerEntry[] entries)
        : this()
    {
        if (entries.Length > 0)
        {
            Shards.AddRange(entries);
        }
    }

    public void AddShard(GameServerEntry entry)
        => Shards.Add(entry);

    protected override bool ParsePayload(ref SpanReader reader)
        => true;

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);

        var length = 6 + (40 * Shards.Count);
        writer.Write((ushort)length);
        writer.Write((byte)0x5D);
        writer.Write((ushort)Shards.Count);

        foreach (var shard in Shards)
        {
            writer.Write(shard.Write().Span);
        }
    }
}
