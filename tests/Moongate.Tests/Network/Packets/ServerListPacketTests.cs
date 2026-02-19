using System.Net;
using Moongate.Network.Packets.Outgoing.Login;
using Moongate.Network.Spans;
using Moongate.UO.Data.Packets.Data;

namespace Moongate.Tests.Network.Packets;

public class ServerListPacketTests
{
    [Test]
    public void AddShard_ShouldAppendShard()
    {
        var packet = new ServerListPacket();

        packet.AddShard(
            new()
            {
                Index = 2,
                ServerName = "Trammel",
                IpAddress = IPAddress.Parse("127.0.0.2")
            }
        );

        Assert.That(packet.Shards.Count, Is.EqualTo(1));
        Assert.That(packet.Shards[0].Index, Is.EqualTo(2));
    }

    [Test]
    public void Write_WithSingleShard_ShouldSerializeHeaderAndEntry()
    {
        var packet = new ServerListPacket(
            new GameServerEntry
            {
                Index = 1,
                ServerName = "Britannia",
                IpAddress = IPAddress.Parse("127.0.0.1")
            }
        );

        var writer = new SpanWriter(128, true);
        packet.Write(ref writer);
        var data = writer.ToArray();
        writer.Dispose();

        Assert.Multiple(
            () =>
            {
                Assert.That(data[0], Is.EqualTo(0xA8));
                Assert.That((data[1] << 8) | data[2], Is.EqualTo(46));
                Assert.That(data[3], Is.EqualTo(0x5D));
                Assert.That((data[4] << 8) | data[5], Is.EqualTo(1));
            }
        );
    }
}
