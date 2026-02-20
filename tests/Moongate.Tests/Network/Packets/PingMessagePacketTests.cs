using Moongate.Network.Packets.Incoming.Player;
using Moongate.Network.Spans;

namespace Moongate.Tests.Network.Packets;

public class PingMessagePacketTests
{
    [Test]
    public void Write_ShouldSerializeOpCodeAndSequence()
    {
        var packet = new PingMessagePacket(0x7B);

        var data = Write(packet);

        Assert.That(data, Is.EqualTo(new byte[] { 0x73, 0x7B }));
    }

    [Test]
    public void TryParse_ShouldPopulateSequence()
    {
        var packet = new PingMessagePacket();

        var parsed = packet.TryParse(new byte[] { 0x73, 0x2A });

        Assert.Multiple(
            () =>
            {
                Assert.That(parsed, Is.True);
                Assert.That(packet.Sequence, Is.EqualTo(0x2A));
            }
        );
    }

    private static byte[] Write(PingMessagePacket packet)
    {
        var writer = new SpanWriter(8, true);
        packet.Write(ref writer);
        var data = writer.ToArray();
        writer.Dispose();

        return data;
    }
}
