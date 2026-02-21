using System.Buffers.Binary;
using System.Text;
using Moongate.Network.Packets.Incoming.UI;

namespace Moongate.Tests.Network.Packets;

public class GumpMenuSelectionPacketTests
{
    [Test]
    public void TryParse_ShouldParseSwitchesAndTextEntries()
    {
        var packet = new GumpMenuSelectionPacket();
        var bytes = BuildPacketBytes(
            serial: 0x00000011,
            gumpId: 0x000001CD,
            buttonId: 2,
            switches: [10, 20],
            textEntries: new Dictionary<ushort, string>
            {
                [1] = "hello",
                [2] = "world"
            }
        );

        var ok = packet.TryParse(bytes);

        Assert.Multiple(
            () =>
            {
                Assert.That(ok, Is.True);
                Assert.That(packet.Serial, Is.EqualTo(0x00000011u));
                Assert.That(packet.GumpId, Is.EqualTo(0x000001CDu));
                Assert.That(packet.ButtonId, Is.EqualTo(2u));
                Assert.That(packet.Switches, Is.EqualTo(new uint[] { 10, 20 }));
                Assert.That(packet.TextEntries.Count, Is.EqualTo(2));
                Assert.That(packet.TextEntries[1], Is.EqualTo("hello"));
                Assert.That(packet.TextEntries[2], Is.EqualTo("world"));
            }
        );
    }

    [Test]
    public void TryParse_ShouldFail_WhenDeclaredLengthIsInvalid()
    {
        var packet = new GumpMenuSelectionPacket();
        var bytes = BuildPacketBytes(
            serial: 1,
            gumpId: 2,
            buttonId: 3,
            switches: [],
            textEntries: new Dictionary<ushort, string>()
        );
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(1, 2), 0xFFFF);

        var ok = packet.TryParse(bytes);

        Assert.That(ok, Is.False);
    }

    [Test]
    public void TryParse_ShouldFail_WhenTextEntryLengthExceedsPayload()
    {
        var packet = new GumpMenuSelectionPacket();
        var bytes = BuildPacketBytes(
            serial: 1,
            gumpId: 2,
            buttonId: 3,
            switches: [],
            textEntries: new Dictionary<ushort, string>
            {
                [9] = "x"
            }
        );

        var textLengthOffset = 3 + 4 + 4 + 4 + 4 + 4 + 2;
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(textLengthOffset, 2), 1000);

        var ok = packet.TryParse(bytes);

        Assert.That(ok, Is.False);
    }

    private static byte[] BuildPacketBytes(
        uint serial,
        uint gumpId,
        uint buttonId,
        IReadOnlyList<uint> switches,
        IReadOnlyDictionary<ushort, string> textEntries
    )
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);

        bw.Write((byte)0xB1);
        bw.Write((ushort)0);
        WriteUInt32BE(bw, serial);
        WriteUInt32BE(bw, gumpId);
        WriteUInt32BE(bw, buttonId);
        WriteInt32BE(bw, switches.Count);

        foreach (var value in switches)
        {
            WriteUInt32BE(bw, value);
        }

        WriteInt32BE(bw, textEntries.Count);

        foreach (var (id, text) in textEntries)
        {
            var value = text ?? string.Empty;
            var textBytes = Encoding.BigEndianUnicode.GetBytes(value);

            WriteUInt16BE(bw, id);
            WriteUInt16BE(bw, (ushort)value.Length);
            bw.Write(textBytes);
        }

        bw.Flush();
        var bytes = ms.ToArray();
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(1, 2), (ushort)bytes.Length);

        return bytes;
    }

    private static void WriteUInt16BE(BinaryWriter writer, ushort value)
    {
        Span<byte> buffer = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
        writer.Write(buffer);
    }

    private static void WriteUInt32BE(BinaryWriter writer, uint value)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
        writer.Write(buffer);
    }

    private static void WriteInt32BE(BinaryWriter writer, int value)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(buffer, value);
        writer.Write(buffer);
    }
}
