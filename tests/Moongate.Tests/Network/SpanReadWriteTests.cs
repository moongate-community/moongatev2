using System.IO;
using System.Text;
using Moongate.Network.Spans;

namespace Moongate.Tests.Network;

public class SpanReadWriteTests
{
    [Test]
    public void SpanWriterAndSpanReader_ShouldRoundTripBigEndianPrimitives()
    {
        using var writer = new SpanWriter(64);
        writer.Write((byte)0x2A);
        writer.Write((short)0x1234);
        writer.Write(0x12345678);
        writer.Write(0x0102030405060708L);

        using var reader = new SpanReader(writer.Span);
        Assert.That(reader.ReadByte(), Is.EqualTo(0x2A));
        Assert.That(reader.ReadInt16(), Is.EqualTo(0x1234));
        Assert.That(reader.ReadInt32(), Is.EqualTo(0x12345678));
        Assert.That(reader.ReadInt64(), Is.EqualTo(0x0102030405060708L));
    }

    [Test]
    public void SpanReader_ReadBigUni_ShouldAdvancePositionByTwoByteTerminator()
    {
        var payload = new byte[]
        {
            0x00, 0x41, // A
            0x00, 0x42, // B
            0x00, 0x00, // terminator
            0x00, 0x43  // C
        };

        using var reader = new SpanReader(payload);
        var value = reader.ReadBigUni();
        Assert.That(value, Is.EqualTo("AB"));
        Assert.That(reader.Position, Is.EqualTo(6));
        Assert.That(reader.ReadBigUni(1), Is.EqualTo("C"));
    }

    [Test]
    public void SpanWriter_SeekAndWritePacketLength_ShouldWriteLengthAtOffset1()
    {
        using var writer = new SpanWriter(16, resize: true);
        writer.Write((byte)0xAA);
        writer.Write((ushort)0);
        writer.Write(Encoding.ASCII.GetBytes("TEST"));

        writer.WritePacketLength();
        var bytes = writer.ToArray();

        Assert.Multiple(
            () =>
            {
                Assert.That(bytes[0], Is.EqualTo(0xAA));
                Assert.That(bytes[1], Is.EqualTo(0x00));
                Assert.That(bytes[2], Is.EqualTo(0x07));
            }
        );
    }

    [Test]
    public void SpanWriter_WhenResizeDisabledAndCapacityExceeded_ShouldThrowInvalidOperationException()
    {
        Span<byte> fixedBuffer = stackalloc byte[2];
        using var writer = new SpanWriter(fixedBuffer, resize: false);
        writer.Write((ushort)1);
        var didThrow = false;
        try
        {
            writer.Write((byte)1);
        }
        catch (InvalidOperationException)
        {
            didThrow = true;
        }

        Assert.That(didThrow, Is.True);
    }

    [Test]
    public void SpanReader_SeekWithInvalidBounds_ShouldThrowIOException()
    {
        using var reader = new SpanReader(new byte[] { 0x01, 0x02 });
        var didThrow = false;
        try
        {
            reader.Seek(5, SeekOrigin.Begin);
        }
        catch (IOException)
        {
            didThrow = true;
        }

        Assert.That(didThrow, Is.True);
    }
}
