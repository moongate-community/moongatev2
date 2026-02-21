using System.Buffers.Binary;
using Moongate.Network.Packets.Incoming.Books;
using Moongate.Network.Spans;

namespace Moongate.Tests.Network.Packets;

public class BookPagesPacketTests
{
    [Test]
    public void TryParse_WithPageRequest_ShouldReadSentinelLineCount()
    {
        var raw = BuildPacket(
            serial: 0x40000031u,
            pages: new[] { (page: (ushort)3, lines: (ushort)0xFFFF, text: Array.Empty<string>()) }
        );

        var packet = new BookPagesPacket();
        var ok = packet.TryParse(raw);

        Assert.Multiple(
            () =>
            {
                Assert.That(ok, Is.True);
                Assert.That(packet.BookSerial, Is.EqualTo(0x40000031u));
                Assert.That(packet.PageCount, Is.EqualTo((ushort)1));
                Assert.That(packet.Pages.Count, Is.EqualTo(1));
                Assert.That(packet.Pages[0].PageNumber, Is.EqualTo((ushort)3));
                Assert.That(packet.Pages[0].LineCount, Is.EqualTo((ushort)0xFFFF));
                Assert.That(packet.Pages[0].Lines, Is.Empty);
            }
        );
    }

    [Test]
    public void TryParse_WithPageContent_ShouldReadAllLines()
    {
        var lines = new[] { "line one", "line two" };
        var raw = BuildPacket(
            serial: 0x40000032u,
            pages: new[] { (page: (ushort)1, lines: (ushort)2, text: lines) }
        );

        var packet = new BookPagesPacket();
        var ok = packet.TryParse(raw);

        Assert.Multiple(
            () =>
            {
                Assert.That(ok, Is.True);
                Assert.That(packet.BookSerial, Is.EqualTo(0x40000032u));
                Assert.That(packet.PageCount, Is.EqualTo((ushort)1));
                Assert.That(packet.Pages.Count, Is.EqualTo(1));
                Assert.That(packet.Pages[0].PageNumber, Is.EqualTo((ushort)1));
                Assert.That(packet.Pages[0].LineCount, Is.EqualTo((ushort)2));
                Assert.That(packet.Pages[0].Lines, Is.EqualTo(new[] { "line one", "line two" }));
            }
        );
    }

    [Test]
    public void Write_ShouldRoundtripWithTryParse()
    {
        var source = new BookPagesPacket
        {
            BookSerial = 0x40000033u,
            Pages =
            {
                new BookPageEntry { PageNumber = 1, LineCount = 2, Lines = { "first", "second" } },
                new BookPageEntry { PageNumber = 2, LineCount = 0xFFFF }
            }
        };

        var writer = new SpanWriter(256, true);
        source.Write(ref writer);
        var bytes = writer.ToArray();
        writer.Dispose();

        var parsed = new BookPagesPacket();
        var ok = parsed.TryParse(bytes);

        Assert.Multiple(
            () =>
            {
                Assert.That(ok, Is.True);
                Assert.That(parsed.BookSerial, Is.EqualTo(0x40000033u));
                Assert.That(parsed.PageCount, Is.EqualTo((ushort)2));
                Assert.That(parsed.Pages.Count, Is.EqualTo(2));
                Assert.That(parsed.Pages[0].Lines, Is.EqualTo(new[] { "first", "second" }));
                Assert.That(parsed.Pages[1].LineCount, Is.EqualTo((ushort)0xFFFF));
                Assert.That(parsed.Pages[1].Lines, Is.Empty);
            }
        );
    }

    [Test]
    public void TryParse_WithInvalidDeclaredLength_ShouldFail()
    {
        var lines = new[] { "a" };
        var raw = BuildPacket(
            serial: 0x40000034u,
            pages: new[] { (page: (ushort)1, lines: (ushort)1, text: lines) }
        );
        BinaryPrimitives.WriteUInt16BigEndian(raw.AsSpan(1, 2), (ushort)(raw.Length - 1));

        var packet = new BookPagesPacket();
        var ok = packet.TryParse(raw);

        Assert.That(ok, Is.False);
    }

    private static byte[] BuildPacket(
        uint serial,
        (ushort page, ushort lines, string[] text)[] pages
    )
    {
        var writer = new SpanWriter(512, true);
        writer.Write((byte)0x66);
        writer.Write((ushort)0);
        writer.Write(serial);
        writer.Write((ushort)pages.Length);

        foreach (var (page, lines, text) in pages)
        {
            writer.Write(page);
            writer.Write(lines);

            if (lines == 0xFFFF)
            {
                continue;
            }

            for (var i = 0; i < lines; i++)
            {
                writer.WriteUTF8Null(i < text.Length ? text[i] : string.Empty);
            }
        }

        writer.WritePacketLength();
        var bytes = writer.ToArray();
        writer.Dispose();

        return bytes;
    }
}
