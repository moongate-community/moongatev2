using Moongate.Network.Packets.Outgoing.Speech;
using Moongate.Network.Spans;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.Network.Packets;

public class UnicodeSpeechMessagePacketTests
{
    [Test]
    public void Write_ShouldSerializeUnicodeSpeechMessage()
    {
        var packet = new UnicodeSpeechMessagePacket
        {
            Serial = (Serial)0x00000002,
            Graphic = 0x0190,
            MessageType = ChatMessageType.Regular,
            Hue = 0x0035,
            Font = 0x0003,
            Language = "ENU",
            Name = "Moongate",
            Text = "Welcome"
        };

        var data = Write(packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(data[0], Is.EqualTo(0xAE));
                Assert.That((data[1] << 8) | data[2], Is.EqualTo(data.Length));
                Assert.That(data.Length, Is.GreaterThan(40));
            }
        );
    }

    [Test]
    public void TryParse_ShouldReadSerializedUnicodeSpeechMessage()
    {
        var source = new UnicodeSpeechMessagePacket
        {
            Serial = (Serial)0x00000002,
            Graphic = 0x0190,
            MessageType = ChatMessageType.System,
            Hue = 0x0482,
            Font = 0x0003,
            Language = "ENU",
            Name = "System",
            Text = "Shard online"
        };

        var data = Write(source);

        var parsed = new UnicodeSpeechMessagePacket();
        var success = parsed.TryParse(data);

        Assert.Multiple(
            () =>
            {
                Assert.That(success, Is.True);
                Assert.That(parsed.Serial, Is.EqualTo((Serial)0x00000002));
                Assert.That(parsed.Graphic, Is.EqualTo(0x0190));
                Assert.That(parsed.MessageType, Is.EqualTo(ChatMessageType.System));
                Assert.That(parsed.Hue, Is.EqualTo(0x0482));
                Assert.That(parsed.Font, Is.EqualTo(0x0003));
                Assert.That(parsed.Language, Is.EqualTo("ENU"));
                Assert.That(parsed.Name.TrimEnd('\0'), Is.EqualTo("System"));
                Assert.That(parsed.Text, Is.EqualTo("Shard online"));
            }
        );
    }

    private static byte[] Write(UnicodeSpeechMessagePacket packet)
    {
        var writer = new SpanWriter(256, true);
        packet.Write(ref writer);
        var data = writer.ToArray();
        writer.Dispose();

        return data;
    }
}
