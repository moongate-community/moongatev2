using Moongate.Network.Packets.Outgoing.Speech;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Types;
using Moongate.UO.Data.Utils;

namespace Moongate.Tests.Network.Packets;

public class SpeechMessageFactoryTests
{
    [Test]
    public void GetMaxMessageLength_ShouldMatchProtocolFormula()
    {
        var maxLength = SpeechMessageFactory.GetMaxMessageLength("abc");

        Assert.That(maxLength, Is.EqualTo(56));
    }

    [Test]
    public void CreateFromSpeaker_ShouldApplyDefaultsAndSpeakerData()
    {
        var speaker = new UOMobileEntity
        {
            Id = (Serial)0x00000022,
            Name = "Tom",
            BaseBody = 0x0190
        };

        var packet = SpeechMessageFactory.CreateFromSpeaker(
            speaker,
            ChatMessageType.Regular,
            hue: 0,
            font: 0,
            language: null,
            text: "hello"
        );

        Assert.Multiple(
            () =>
            {
                Assert.That(packet.Serial, Is.EqualTo((Serial)0x00000022));
                Assert.That(packet.Name, Is.EqualTo("Tom"));
                Assert.That(packet.Graphic, Is.EqualTo(0x0190));
                Assert.That(packet.Hue, Is.EqualTo(SpeechHues.Default));
                Assert.That(packet.Font, Is.EqualTo(SpeechHues.DefaultFont));
                Assert.That(packet.Language, Is.EqualTo("ENU"));
                Assert.That(packet.Text, Is.EqualTo("hello"));
            }
        );
    }

    [Test]
    public void CreateSystem_ShouldCreateSystemPacket()
    {
        var packet = SpeechMessageFactory.CreateSystem("Shard online");

        Assert.Multiple(
            () =>
            {
                Assert.That(packet.Serial, Is.EqualTo(Serial.Zero));
                Assert.That(packet.Name, Is.EqualTo("System"));
                Assert.That(packet.MessageType, Is.EqualTo(ChatMessageType.System));
                Assert.That(packet.Hue, Is.EqualTo(SpeechHues.System));
                Assert.That(packet.Font, Is.EqualTo(SpeechHues.DefaultFont));
                Assert.That(packet.Text, Is.EqualTo("Shard online"));
            }
        );
    }

    [Test]
    public void CreateWhisperFromSpeaker_ShouldSetWhisperMessageType()
    {
        var speaker = new UOMobileEntity
        {
            Id = (Serial)0x00000033,
            Name = "Whisperer",
            BaseBody = 0x0190
        };

        var packet = SpeechMessageFactory.CreateWhisperFromSpeaker(speaker, "psst");

        Assert.Multiple(
            () =>
            {
                Assert.That(packet.MessageType, Is.EqualTo(ChatMessageType.Whisper));
                Assert.That(packet.Text, Is.EqualTo("psst"));
                Assert.That(packet.Name, Is.EqualTo("Whisperer"));
            }
        );
    }

    [Test]
    public void CreateYellFromSpeaker_ShouldSetYellMessageType()
    {
        var speaker = new UOMobileEntity
        {
            Id = (Serial)0x00000044,
            Name = "Yeller",
            BaseBody = 0x0190
        };

        var packet = SpeechMessageFactory.CreateYellFromSpeaker(speaker, "HEY");

        Assert.Multiple(
            () =>
            {
                Assert.That(packet.MessageType, Is.EqualTo(ChatMessageType.Yell));
                Assert.That(packet.Text, Is.EqualTo("HEY"));
                Assert.That(packet.Name, Is.EqualTo("Yeller"));
            }
        );
    }

    [Test]
    public void CreateMessageBytes_ShouldSerializeUnicodeSpeechPacketWithLength()
    {
        var packet = SpeechMessageFactory.CreateSystem("Shard online");

        var data = SpeechMessageFactory.CreateMessageBytes(packet);
        var declaredLength = (data[1] << 8) | data[2];

        Assert.Multiple(
            () =>
            {
                Assert.That(data[0], Is.EqualTo(0xAE));
                Assert.That(declaredLength, Is.EqualTo(data.Length));
            }
        );
    }
}
