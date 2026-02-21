using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Types;
using Moongate.UO.Data.Utils;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Outgoing.Speech;

/// <summary>
/// Factory helpers for creating outbound unicode speech packets with consistent defaults.
/// </summary>
public static class SpeechMessageFactory
{
    private const int StackallocThreshold = 512;

    /// <summary>
    /// Gets the maximum packet buffer length needed for a unicode speech message payload.
    /// </summary>
    /// <param name="text">Speech text.</param>
    /// <returns>Maximum packet length in bytes.</returns>
    public static int GetMaxMessageLength(string? text)
        => 50 + (text?.Length ?? 0) * 2;

    /// <summary>
    /// Creates a speech packet emitted by a mobile speaker.
    /// </summary>
    /// <param name="speaker">Speaker mobile, or <c>null</c> for system-like fallback metadata.</param>
    /// <param name="messageType">Speech message type.</param>
    /// <param name="hue">Requested speech hue. Uses default speech hue when zero.</param>
    /// <param name="font">Requested speech font. Uses default speech font when zero.</param>
    /// <param name="language">Language code. Uses <c>ENU</c> when missing.</param>
    /// <param name="text">Speech text.</param>
    /// <returns>A configured unicode speech message packet.</returns>
    public static UnicodeSpeechMessagePacket CreateFromSpeaker(
        UOMobileEntity? speaker,
        ChatMessageType messageType,
        short hue,
        short font,
        string? language,
        string text
    )
    {
        return new()
        {
            Serial = speaker?.Id ?? Serial.MinusOne,
            Graphic = (ushort)(speaker?.Body.BodyID ?? SpeechHues.DefaultGraphic),
            MessageType = messageType,
            Hue = hue == 0 ? SpeechHues.Default : hue,
            Font = font == 0 ? (short)SpeechHues.DefaultFont : font,
            Language = string.IsNullOrWhiteSpace(language) ? "ENU" : language,
            Name = speaker?.Name ?? "System",
            Text = text
        };
    }

    /// <summary>
    /// Creates a regular speech (say) packet emitted by a mobile speaker.
    /// </summary>
    /// <param name="speaker">Speaker mobile.</param>
    /// <param name="text">Speech text.</param>
    /// <param name="hue">Speech hue.</param>
    /// <param name="font">Speech font.</param>
    /// <param name="language">Language code.</param>
    /// <returns>A configured unicode speech message packet.</returns>
    public static UnicodeSpeechMessagePacket CreateSayFromSpeaker(
        UOMobileEntity? speaker,
        string text,
        short hue = SpeechHues.Default,
        short font = SpeechHues.DefaultFont,
        string language = "ENU"
    )
    {
        return CreateFromSpeaker(speaker, ChatMessageType.Regular, hue, font, language, text);
    }

    /// <summary>
    /// Creates a whisper speech packet emitted by a mobile speaker.
    /// </summary>
    /// <param name="speaker">Speaker mobile.</param>
    /// <param name="text">Speech text.</param>
    /// <param name="hue">Speech hue.</param>
    /// <param name="font">Speech font.</param>
    /// <param name="language">Language code.</param>
    /// <returns>A configured unicode speech message packet.</returns>
    public static UnicodeSpeechMessagePacket CreateWhisperFromSpeaker(
        UOMobileEntity? speaker,
        string text,
        short hue = SpeechHues.Default,
        short font = SpeechHues.DefaultFont,
        string language = "ENU"
    )
    {
        return CreateFromSpeaker(speaker, ChatMessageType.Whisper, hue, font, language, text);
    }

    /// <summary>
    /// Creates a yell speech packet emitted by a mobile speaker.
    /// </summary>
    /// <param name="speaker">Speaker mobile.</param>
    /// <param name="text">Speech text.</param>
    /// <param name="hue">Speech hue.</param>
    /// <param name="font">Speech font.</param>
    /// <param name="language">Language code.</param>
    /// <returns>A configured unicode speech message packet.</returns>
    public static UnicodeSpeechMessagePacket CreateYellFromSpeaker(
        UOMobileEntity? speaker,
        string text,
        short hue = SpeechHues.Default,
        short font = SpeechHues.DefaultFont,
        string language = "ENU"
    )
    {
        return CreateFromSpeaker(speaker, ChatMessageType.Yell, hue, font, language, text);
    }

    /// <summary>
    /// Creates a system speech packet using standard system metadata.
    /// </summary>
    /// <param name="text">System message text.</param>
    /// <param name="hue">System message hue.</param>
    /// <param name="font">System message font.</param>
    /// <param name="language">System message language code.</param>
    /// <returns>A configured unicode speech packet marked as system message.</returns>
    public static UnicodeSpeechMessagePacket CreateSystem(
        string text,
        short hue = SpeechHues.System,
        short font = SpeechHues.DefaultFont,
        string language = "ENU"
    )
    {
        return new()
        {
            Serial = Serial.Zero,
            Graphic = (ushort)SpeechHues.DefaultGraphic,
            MessageType = ChatMessageType.System,
            Hue = hue,
            Font = font,
            Language = language,
            Name = "System",
            Text = text
        };
    }

    /// <summary>
    /// Creates a unicode speech packet payload into the provided destination buffer.
    /// </summary>
    /// <param name="destination">Destination buffer.</param>
    /// <param name="packet">Source packet metadata.</param>
    /// <returns>Bytes written.</returns>
    public static int CreateMessage(Span<byte> destination, UnicodeSpeechMessagePacket packet)
    {
        return CreateMessage(
            destination,
            packet.Serial,
            packet.Graphic,
            packet.MessageType,
            packet.Hue,
            packet.Font,
            packet.Language,
            packet.Name,
            packet.Text
        );
    }

    /// <summary>
    /// Creates a unicode speech packet payload into the provided destination buffer.
    /// </summary>
    /// <param name="destination">Destination buffer.</param>
    /// <param name="serial">Speaker serial.</param>
    /// <param name="graphic">Speaker body/graphic.</param>
    /// <param name="messageType">Speech message type.</param>
    /// <param name="hue">Speech hue.</param>
    /// <param name="font">Speech font.</param>
    /// <param name="language">Language code.</param>
    /// <param name="name">Speaker name.</param>
    /// <param name="text">Speech text.</param>
    /// <returns>Bytes written.</returns>
    public static int CreateMessage(
        Span<byte> destination,
        Serial serial,
        int graphic,
        ChatMessageType messageType,
        int hue,
        int font,
        string? language,
        string? name,
        string? text
    )
    {
        language ??= "ENU";
        name ??= string.Empty;
        text ??= string.Empty;

        if (hue == 0)
        {
            hue = SpeechHues.Default;
        }

        var writer = new SpanWriter(destination);
        writer.Write((byte)0xAE);
        writer.Seek(2, SeekOrigin.Current);
        writer.Write(serial.Value);
        writer.Write((ushort)graphic);
        writer.Write((byte)messageType);
        writer.Write((ushort)hue);
        writer.Write((ushort)font);
        writer.WriteAscii(language, 4);
        writer.WriteAscii(name, 30);
        writer.WriteBigUniNull(text);
        writer.WritePacketLength();

        return writer.Position;
    }

    /// <summary>
    /// Creates a byte array payload for the provided unicode speech message packet.
    /// </summary>
    /// <param name="packet">Speech packet to serialize.</param>
    /// <returns>Serialized packet bytes.</returns>
    public static byte[] CreateMessageBytes(UnicodeSpeechMessagePacket packet)
    {
        var maxLength = GetMaxMessageLength(packet.Text);

        if (maxLength <= StackallocThreshold)
        {
            Span<byte> buffer = stackalloc byte[maxLength];
            var length = CreateMessage(buffer, packet);

            return buffer[..length].ToArray();
        }

        var heapBuffer = new byte[maxLength];
        var written = CreateMessage(heapBuffer, packet);

        return heapBuffer.AsSpan(0, written).ToArray();
    }
}
