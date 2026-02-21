using Moongate.Network.Packets.Incoming.Speech;
using Moongate.Network.Spans;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.Network.Packets;

public class UnicodeSpeechPacketTests
{
    [Test]
    public void TryParse_ShouldPopulateFields_ForRegularUnicodeSpeech()
    {
        var packet = new UnicodeSpeechPacket();
        var data = BuildPayload(
            ChatMessageType.Regular,
            hue: 0x0035,
            font: 0x0003,
            language: "ENU",
            text: "hello"
        );

        var parsed = packet.TryParse(data);

        Assert.Multiple(
            () =>
            {
                Assert.That(parsed, Is.True);
                Assert.That(packet.MessageType, Is.EqualTo(ChatMessageType.Regular));
                Assert.That(packet.Hue, Is.EqualTo(0x0035));
                Assert.That(packet.Font, Is.EqualTo(0x0003));
                Assert.That(packet.Language, Is.EqualTo("ENU"));
                Assert.That(packet.Text, Is.EqualTo("hello"));
                Assert.That(packet.Keywords, Is.Empty);
            }
        );
    }

    [Test]
    public void TryParse_ShouldReturnFalse_WhenDeclaredLengthDoesNotMatchBuffer()
    {
        var packet = new UnicodeSpeechPacket();

        var data = BuildPayload(
            ChatMessageType.Regular,
            hue: 0x0035,
            font: 0x0003,
            language: "ENU",
            text: "hello"
        );

        data[1] = 0x00;
        data[2] = 0x03;

        var parsed = packet.TryParse(data);

        Assert.That(parsed, Is.False);
    }

    private static byte[] BuildPayload(ChatMessageType messageType, short hue, short font, string language, string text)
    {
        var writer = new SpanWriter(128, true);

        writer.Write((byte)0xAD);
        writer.Write((ushort)0);
        writer.Write((byte)messageType);
        writer.Write(hue);
        writer.Write(font);
        writer.WriteAscii(language, 4);
        writer.WriteBigUniNull(text);
        writer.WritePacketLength();

        var data = writer.ToArray();
        writer.Dispose();

        return data;
    }
}
