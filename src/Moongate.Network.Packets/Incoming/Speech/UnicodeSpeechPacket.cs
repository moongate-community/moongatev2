using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Types;

namespace Moongate.Network.Packets.Incoming.Speech;

[PacketHandler(0xAD, PacketSizing.Variable, Description = "Unicode/Ascii speech request")]
public class UnicodeSpeechPacket : BaseGameNetworkPacket
{
    public short Font { get; set; }

    public short Hue { get; set; }

    public int[] Keywords { get; set; }

    public string Language { get; set; }

    public ChatMessageType MessageType { get; set; }

    public string Text { get; set; }

    public UnicodeSpeechPacket()
        : base(0xAD)
    {
        Keywords = Array.Empty<int>();
        Language = "ENU";
        Text = string.Empty;
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining < 2)
        {
            return false;
        }

        var declaredLength = reader.ReadUInt16();

        if (declaredLength != reader.Length)
        {
            return false;
        }

        if (declaredLength == 3 && reader.Remaining == 0)
        {
            Keywords = Array.Empty<int>();
            MessageType = ChatMessageType.Regular;
            Hue = 0;
            Font = 0;
            Language = "ENU";
            Text = string.Empty;

            return true;
        }

        if (reader.Remaining < 9)
        {
            return false;
        }

        MessageType = (ChatMessageType)reader.ReadByte();
        Hue = reader.ReadInt16();
        Font = reader.ReadInt16();
        Language = reader.ReadAscii(4);

        var isEncoded = (MessageType & ChatMessageType.Encoded) != 0;

        if (isEncoded)
        {
            if (!TryReadEncodedSpeech(ref reader, out var text, out var keywords))
            {
                return false;
            }

            Text = text.Trim();
            Keywords = keywords;
        }
        else
        {
            Text = reader.ReadBigUniSafe().Trim();
            Keywords = Array.Empty<int>();
        }

        MessageType &= ~ChatMessageType.Encoded;

        if (!Enum.IsDefined(MessageType))
        {
            MessageType = ChatMessageType.Regular;
        }

        return Text.Length is > 0 and <= 128;
    }

    private static bool TryReadEncodedSpeech(ref SpanReader reader, out string text, out int[] keywords)
    {
        text = string.Empty;
        keywords = Array.Empty<int>();

        if (reader.Remaining < 2)
        {
            return false;
        }

        var packed = reader.ReadUInt16();
        var count = (packed & 0xFFF0) >> 4;
        var hold = packed & 0x000F;

        if (count > 50)
        {
            return false;
        }

        var keywordSet = new HashSet<int>();

        for (var i = 0; i < count; i++)
        {
            int keyword;

            if ((i & 1) == 0)
            {
                if (reader.Remaining < 1)
                {
                    return false;
                }

                hold <<= 8;
                hold |= reader.ReadByte();
                keyword = hold;
                hold = 0;
            }
            else
            {
                if (reader.Remaining < 2)
                {
                    return false;
                }

                packed = reader.ReadUInt16();
                keyword = (packed & 0xFFF0) >> 4;
                hold = packed & 0x000F;
            }

            keywordSet.Add(keyword);
        }

        text = reader.ReadUTF8Safe();
        keywords = keywordSet.Count == 0 ? Array.Empty<int>() : keywordSet.ToArray();

        return true;
    }
}
