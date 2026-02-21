using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using System.Text;

namespace Moongate.Network.Packets.Incoming.Books;

[PacketHandler(0xD4, PacketSizing.Variable, Description = "Book Header ( New )")]
public class BookHeaderNewPacket : BaseGameNetworkPacket
{
    public uint BookSerial { get; set; }

    public bool Flag1 { get; set; }

    public bool IsWritable { get; set; }

    public ushort PageCount { get; set; }

    public string Title { get; set; }

    public string Author { get; set; }

    public BookHeaderNewPacket()
        : base(0xD4)
    {
        Title = string.Empty;
        Author = string.Empty;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((ushort)0);
        writer.Write(BookSerial);
        writer.Write(Flag1);
        writer.Write(IsWritable);
        writer.Write(PageCount);
        WriteStringField(ref writer, Author);
        WriteStringField(ref writer, Title);
        writer.WritePacketLength();
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining < 12)
        {
            return false;
        }

        var declaredLength = reader.ReadUInt16();

        if (declaredLength != reader.Length || reader.Remaining < 10)
        {
            return false;
        }

        BookSerial = reader.ReadUInt32();
        Flag1 = reader.ReadBoolean();
        IsWritable = reader.ReadBoolean();
        PageCount = reader.ReadUInt16();

        if (!TryReadStringField(ref reader, out var author))
        {
            return false;
        }

        if (!TryReadStringField(ref reader, out var title))
        {
            return false;
        }

        Author = author;
        Title = title;

        return reader.Remaining == 0;
    }

    private static void WriteStringField(ref SpanWriter writer, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        writer.Write((ushort)(bytes.Length + 1));
        writer.Write(bytes);
        writer.Write((byte)0);
    }

    private static bool TryReadStringField(ref SpanReader reader, out string value)
    {
        value = string.Empty;

        if (reader.Remaining < 2)
        {
            return false;
        }

        var fieldLength = reader.ReadUInt16();

        if (fieldLength == 0 || fieldLength > reader.Remaining)
        {
            return false;
        }

        var bytes = reader.ReadBytes(fieldLength);
        value = DecodeStringField(bytes);

        return true;
    }

    private static string DecodeStringField(byte[] bytes)
    {
        if (bytes.Length == 0)
        {
            return string.Empty;
        }

        if (bytes.Length >= 2 && (bytes.Length % 2) == 0 && bytes[^2] == 0 && bytes[^1] == 0)
        {
            return Encoding.Unicode.GetString(bytes, 0, bytes.Length - 2);
        }

        if (bytes[^1] == 0)
        {
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length - 1);
        }

        return Encoding.UTF8.GetString(bytes);
    }
}
