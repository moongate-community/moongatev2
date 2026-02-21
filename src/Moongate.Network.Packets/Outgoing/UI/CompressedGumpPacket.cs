using System.IO.Compression;
using System.Text;
using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Outgoing.UI;

[PacketHandler(0xDD, PacketSizing.Variable, Description = "Compressed Gump")]
public sealed class CompressedGumpPacket : BaseGameNetworkPacket
{
    public uint GumpId { get; set; }

    public string Layout { get; set; } = string.Empty;

    public uint SenderSerial { get; set; }

    public List<string> TextLines { get; } = [];

    public uint X { get; set; }

    public uint Y { get; set; }

    public CompressedGumpPacket()
        : base(0xDD) { }

    public override void Write(ref SpanWriter writer)
    {
        var layoutBytes = BuildLayoutBytes(Layout);
        var compressedLayout = Compress(layoutBytes);

        var stringsBytes = BuildStringsBytes(TextLines);
        var compressedStrings = stringsBytes.Length == 0 ? [] : Compress(stringsBytes);

        writer.Write(OpCode);
        writer.Write((ushort)0);
        writer.Write(SenderSerial);
        writer.Write(GumpId);
        writer.Write(X);
        writer.Write(Y);

        writer.Write((uint)(compressedLayout.Length + 4));
        writer.Write((uint)layoutBytes.Length);
        writer.Write(compressedLayout);

        writer.Write((uint)TextLines.Count);
        if (compressedStrings.Length == 0)
        {
            writer.Write(0u);
        }
        else
        {
            writer.Write((uint)(compressedStrings.Length + 4));
            writer.Write((uint)stringsBytes.Length);
            writer.Write(compressedStrings);
        }

        writer.WritePacketLength();
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining < 24)
        {
            return false;
        }

        var declaredLength = reader.ReadUInt16();
        if (declaredLength != reader.Length)
        {
            return false;
        }

        SenderSerial = reader.ReadUInt32();
        GumpId = reader.ReadUInt32();
        X = reader.ReadUInt32();
        Y = reader.ReadUInt32();

        var layoutCompressedLengthWithHeader = reader.ReadUInt32();
        if (layoutCompressedLengthWithHeader < 4)
        {
            return false;
        }

        var layoutUncompressedLength = reader.ReadUInt32();
        var layoutCompressedLength = (int)layoutCompressedLengthWithHeader - 4;

        if (layoutCompressedLength > reader.Remaining)
        {
            return false;
        }

        var compressedLayout = reader.ReadBytes(layoutCompressedLength);
        var layoutBytes = Decompress(compressedLayout, (int)layoutUncompressedLength);
        Layout = DecodeLayout(layoutBytes);

        if (reader.Remaining < 8)
        {
            return false;
        }

        var textLineCount = reader.ReadUInt32();
        var textCompressedLengthWithHeader = reader.ReadUInt32();
        TextLines.Clear();

        if (textCompressedLengthWithHeader == 0)
        {
            return textLineCount == 0 && reader.Remaining == 0;
        }

        if (textCompressedLengthWithHeader < 4)
        {
            return false;
        }

        if (reader.Remaining < 4)
        {
            return false;
        }

        var textUncompressedLength = reader.ReadUInt32();
        var textCompressedLength = (int)textCompressedLengthWithHeader - 4;

        if (textCompressedLength > reader.Remaining)
        {
            return false;
        }

        var compressedText = reader.ReadBytes(textCompressedLength);
        var textBytes = Decompress(compressedText, (int)textUncompressedLength);
        ParseTextLines(textBytes, (int)textLineCount);

        return reader.Remaining == 0;
    }

    private static byte[] BuildLayoutBytes(string layout)
    {
        var text = layout ?? string.Empty;
        if (text.Length > 0 && text[^1] == '\0')
        {
            return Encoding.ASCII.GetBytes(text);
        }

        var bytes = Encoding.ASCII.GetBytes(text);
        var withTerminator = new byte[bytes.Length + 1];
        bytes.CopyTo(withTerminator, 0);

        return withTerminator;
    }

    private static byte[] BuildStringsBytes(IReadOnlyList<string> lines)
    {
        if (lines.Count == 0)
        {
            return [];
        }

        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);

        foreach (var line in lines)
        {
            var text = line ?? string.Empty;
            writer.Write((byte)(text.Length >> 8));
            writer.Write((byte)(text.Length & 0xFF));
            var bytes = Encoding.BigEndianUnicode.GetBytes(text);
            writer.Write(bytes);
        }

        writer.Flush();
        return ms.ToArray();
    }

    private static byte[] Compress(ReadOnlySpan<byte> input)
    {
        using var ms = new MemoryStream();
        using (var zlib = new ZLibStream(ms, CompressionLevel.Optimal, leaveOpen: true))
        {
            zlib.Write(input);
        }

        return ms.ToArray();
    }

    private static byte[] Decompress(ReadOnlySpan<byte> input, int expectedLength)
    {
        using var inputStream = new MemoryStream(input.ToArray());
        using var zlib = new ZLibStream(inputStream, CompressionMode.Decompress);
        using var output = new MemoryStream(expectedLength > 0 ? expectedLength : 0);
        zlib.CopyTo(output);
        var result = output.ToArray();

        if (expectedLength > 0 && result.Length != expectedLength)
        {
            throw new InvalidOperationException("Compressed gump payload decompressed with an unexpected length.");
        }

        return result;
    }

    private static string DecodeLayout(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 0)
        {
            return string.Empty;
        }

        var terminatorIndex = bytes.IndexOf((byte)0);
        var content = terminatorIndex >= 0 ? bytes[..terminatorIndex] : bytes;
        return Encoding.ASCII.GetString(content);
    }

    private void ParseTextLines(ReadOnlySpan<byte> textBytes, int expectedCount)
    {
        var reader = new SpanReader(textBytes);

        for (var i = 0; i < expectedCount; i++)
        {
            if (reader.Remaining < 2)
            {
                throw new InvalidOperationException("Invalid compressed gump string payload.");
            }

            var charLength = reader.ReadUInt16();
            var bytesLength = checked((int)charLength * 2);

            if (reader.Remaining < bytesLength)
            {
                throw new InvalidOperationException("Invalid compressed gump string length.");
            }

            var bytes = reader.ReadBytes(bytesLength);
            TextLines.Add(Encoding.BigEndianUnicode.GetString(bytes));
        }

        if (reader.Remaining != 0)
        {
            throw new InvalidOperationException("Compressed gump string payload has trailing bytes.");
        }
    }
}
