using System.Buffers.Binary;
using Moongate.Network.Packets.Incoming.Books;
using Moongate.Network.Spans;

namespace Moongate.Tests.Network.Packets;

public class BookHeaderNewPacketTests
{
    [Test]
    public void TryParse_WithValidPayload_ShouldPopulateFields()
    {
        var raw = BuildRawPacket(
            0x40000010u,
            flag1: true,
            isWritable: true,
            pageCount: 64,
            author: "Moongate Team",
            title: "Roadmap"
        );

        var packet = new BookHeaderNewPacket();
        var ok = packet.TryParse(raw);

        Assert.Multiple(
            () =>
            {
                Assert.That(ok, Is.True);
                Assert.That(packet.BookSerial, Is.EqualTo(0x40000010u));
                Assert.That(packet.Flag1, Is.True);
                Assert.That(packet.IsWritable, Is.True);
                Assert.That(packet.PageCount, Is.EqualTo((ushort)64));
                Assert.That(packet.Author, Is.EqualTo("Moongate Team"));
                Assert.That(packet.Title, Is.EqualTo("Roadmap"));
            }
        );
    }

    [Test]
    public void TryParse_WithInvalidDeclaredLength_ShouldFail()
    {
        var raw = BuildRawPacket(0x40000010u, true, false, 10, "Author", "Title");
        BinaryPrimitives.WriteUInt16BigEndian(raw.AsSpan(1, 2), (ushort)(raw.Length - 1));

        var packet = new BookHeaderNewPacket();
        var ok = packet.TryParse(raw);

        Assert.That(ok, Is.False);
    }

    [Test]
    public void Write_ShouldRoundtripWithTryParse()
    {
        var source = new BookHeaderNewPacket
        {
            BookSerial = 0x40000020u,
            Flag1 = true,
            IsWritable = false,
            PageCount = 99,
            Author = "Author",
            Title = "Book Title"
        };

        var writer = new SpanWriter(256, true);
        source.Write(ref writer);
        var bytes = writer.ToArray();
        writer.Dispose();

        var parsed = new BookHeaderNewPacket();
        var ok = parsed.TryParse(bytes);

        Assert.Multiple(
            () =>
            {
                Assert.That(ok, Is.True);
                Assert.That(parsed.BookSerial, Is.EqualTo(source.BookSerial));
                Assert.That(parsed.Flag1, Is.EqualTo(source.Flag1));
                Assert.That(parsed.IsWritable, Is.EqualTo(source.IsWritable));
                Assert.That(parsed.PageCount, Is.EqualTo(source.PageCount));
                Assert.That(parsed.Author, Is.EqualTo(source.Author));
                Assert.That(parsed.Title, Is.EqualTo(source.Title));
            }
        );
    }

    private static byte[] BuildRawPacket(
        uint serial,
        bool flag1,
        bool isWritable,
        ushort pageCount,
        string author,
        string title
    )
    {
        var writer = new SpanWriter(256, true);
        writer.Write((byte)0xD4);
        writer.Write((ushort)0);
        writer.Write(serial);
        writer.Write(flag1);
        writer.Write(isWritable);
        writer.Write(pageCount);

        WriteUtf8StringWithLength(ref writer, author);
        WriteUtf8StringWithLength(ref writer, title);
        writer.WritePacketLength();

        var bytes = writer.ToArray();
        writer.Dispose();

        return bytes;
    }

    private static void WriteUtf8StringWithLength(ref SpanWriter writer, string value)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(value);
        writer.Write((ushort)(bytes.Length + 1));
        writer.Write(bytes);
        writer.Write((byte)0);
    }
}
