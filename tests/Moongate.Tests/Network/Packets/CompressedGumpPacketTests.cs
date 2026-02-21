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

    [Test]
    public void TryParse_ShouldFail_WhenDeclaredPacketLengthDoesNotMatchBufferLength()
    {
        var packet = new CompressedGumpPacket
        {
            SenderSerial = 0x00000005,
            GumpId = 0x00000066,
            X = 10,
            Y = 20,
            Layout = "{ page 0 }"
        };

        var bytes = Write(packet);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(1, 2), (ushort)(bytes.Length + 1));

        var parsed = new CompressedGumpPacket();
        var ok = parsed.TryParse(bytes);

        Assert.That(ok, Is.False);
    }

    [Test]
    public void TryParse_ShouldFail_WhenStringsSectionHasInvalidHeaderLength()
    {
        var packet = new CompressedGumpPacket
        {
            SenderSerial = 0x00000006,
            GumpId = 0x00000067,
            X = 1,
            Y = 2,
            Layout = "{ page 0 }"
        };
        packet.TextLines.Add("abc");

        var bytes = Write(packet);
        var offset = FindTextCompressedLengthOffset(bytes);
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(offset, 4), 1);

        var parsed = new CompressedGumpPacket();
        var ok = parsed.TryParse(bytes);

        Assert.That(ok, Is.False);
    }

    [Test]
    public void TryParse_ShouldFail_WhenTextLineCountExceedsStringsPayload()
    {
        var packet = new CompressedGumpPacket
        {
            SenderSerial = 0x00000007,
            GumpId = 0x00000068,
            X = 5,
            Y = 9,
            Layout = "{ page 0 }"
        };
        packet.TextLines.Add("one");

        var bytes = Write(packet);
        var textCountOffset = FindTextLineCountOffset(bytes);
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(textCountOffset, 4), 2);

        var parsed = new CompressedGumpPacket();
        var ok = parsed.TryParse(bytes);

        Assert.That(ok, Is.False);
    }

    [Test]
    public void TryParse_ShouldFail_WhenLayoutCompressedPayloadIsCorrupted()
    {
        var packet = new CompressedGumpPacket
        {
            SenderSerial = 0x00000008,
            GumpId = 0x00000069,
            X = 25,
            Y = 35,
            Layout = "{ page 0 }"
        };

        var bytes = Write(packet);
        bytes[27] ^= 0xFF;

        var parsed = new CompressedGumpPacket();
        Assert.That(() => parsed.TryParse(bytes), Throws.Nothing);
        Assert.That(parsed.TryParse(bytes), Is.False);
    }

    [Test]
    public void TryParse_ShouldFail_WhenStringsPayloadHasTrailingBytes()
    {
        var packet = new CompressedGumpPacket
        {
            SenderSerial = 0x00000009,
            GumpId = 0x00000070,
            X = 7,
            Y = 8,
            Layout = "{ page 0 }"
        };
        packet.TextLines.Add("one");
        packet.TextLines.Add("two");

        var bytes = Write(packet);
        var textCountOffset = FindTextLineCountOffset(bytes);
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(textCountOffset, 4), 1);

        var parsed = new CompressedGumpPacket();
        var ok = parsed.TryParse(bytes);

        Assert.That(ok, Is.False);
    }

    private static byte[] Write(CompressedGumpPacket packet)
    {
        var writer = new SpanWriter(1024, true);
        packet.Write(ref writer);
        var bytes = writer.ToArray();
        writer.Dispose();

        return bytes;
    }

    private static int FindTextLineCountOffset(ReadOnlySpan<byte> packetBytes)
    {
        var layoutLengthWithHeader = BinaryPrimitives.ReadUInt32BigEndian(packetBytes.Slice(19, 4));
        return 27 + (int)layoutLengthWithHeader - 4;
    }

    private static int FindTextCompressedLengthOffset(ReadOnlySpan<byte> packetBytes)
        => FindTextLineCountOffset(packetBytes) + 4;
}
