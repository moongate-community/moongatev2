using System.Buffers.Binary;
using Moongate.Network.Packets.Outgoing.UI;
using Moongate.Network.Spans;

namespace Moongate.Tests.Network.Packets;

public class CompressedGumpPacketTests
{
    [Test]
    public void Write_ShouldSerializeHeaderAndVariableSections()
    {
        var packet = new CompressedGumpPacket
        {
            SenderSerial = 0x00000001,
            GumpId = 0x000001CD,
            X = 120,
            Y = 80,
            Layout = "{ nomove }{ noclose }"
        };
        packet.TextLines.Add("line one");
        packet.TextLines.Add("line two");

        var bytes = Write(packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(bytes[0], Is.EqualTo(0xDD));
                Assert.That(BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(1, 2)), Is.EqualTo((ushort)bytes.Length));
                Assert.That(BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(3, 4)), Is.EqualTo(0x00000001u));
                Assert.That(BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(7, 4)), Is.EqualTo(0x000001CDu));
                Assert.That(BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(11, 4)), Is.EqualTo(120u));
                Assert.That(BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(15, 4)), Is.EqualTo(80u));
            }
        );
    }

    [Test]
    public void WriteAndParse_ShouldRoundtripCompressedGumpData()
    {
        var original = new CompressedGumpPacket
        {
            SenderSerial = 0x00000002,
            GumpId = 0x000001CE,
            X = 250,
            Y = 300,
            Layout = "{ page 0 }{ resizepic 0 0 5054 260 180 }"
        };
        original.TextLines.Add("first");
        original.TextLines.Add("second");
        original.TextLines.Add("third");

        var bytes = Write(original);

        var parsed = new CompressedGumpPacket();
        var ok = parsed.TryParse(bytes);

        Assert.Multiple(
            () =>
            {
                Assert.That(ok, Is.True);
                Assert.That(parsed.SenderSerial, Is.EqualTo(original.SenderSerial));
                Assert.That(parsed.GumpId, Is.EqualTo(original.GumpId));
                Assert.That(parsed.X, Is.EqualTo(original.X));
                Assert.That(parsed.Y, Is.EqualTo(original.Y));
                Assert.That(parsed.Layout, Is.EqualTo(original.Layout));
                Assert.That(parsed.TextLines, Is.EqualTo(original.TextLines));
            }
        );
    }

    [Test]
    public void WriteAndParse_ShouldHandleEmptyStringsSection()
    {
        var original = new CompressedGumpPacket
        {
            SenderSerial = 0x00000003,
            GumpId = 0x000001CF,
            X = 10,
            Y = 20,
            Layout = "{ gumppic 20 20 10400 }"
        };

        var bytes = Write(original);
        var parsed = new CompressedGumpPacket();
        var ok = parsed.TryParse(bytes);

        Assert.Multiple(
            () =>
            {
                Assert.That(ok, Is.True);
                Assert.That(parsed.Layout, Is.EqualTo(original.Layout));
                Assert.That(parsed.TextLines, Is.Empty);
            }
        );
    }

    private static byte[] Write(CompressedGumpPacket packet)
    {
        var writer = new SpanWriter(1024, true);
        packet.Write(ref writer);
        var bytes = writer.ToArray();
        writer.Dispose();

        return bytes;
    }
}
