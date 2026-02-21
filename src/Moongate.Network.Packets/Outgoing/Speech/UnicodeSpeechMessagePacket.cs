using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Types;
using Moongate.UO.Data.Utils;

namespace Moongate.Network.Packets.Outgoing.Speech;

[PacketHandler(0xAE, PacketSizing.Variable, Description = "Unicode Speech message")]
public class UnicodeSpeechMessagePacket : BaseGameNetworkPacket
{
    public short Font { get; set; }

    public ushort Graphic { get; set; }

    public short Hue { get; set; }

    public string Language { get; set; }

    public ChatMessageType MessageType { get; set; }

    public string Name { get; set; }

    public Serial Serial { get; set; }

    public string Text { get; set; }

    public UnicodeSpeechMessagePacket()
        : base(0xAE)
    {
        Font = SpeechHues.DefaultFont;
        Graphic = (ushort)SpeechHues.DefaultGraphic;
        Hue = SpeechHues.Default;
        Language = "ENU";
        MessageType = ChatMessageType.Regular;
        Name = string.Empty;
        Serial = Serial.MinusOne;
        Text = string.Empty;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((ushort)0);
        writer.Write(Serial.Value);
        writer.Write(Graphic);
        writer.Write((byte)MessageType);
        writer.Write((ushort)(Hue == 0 ? SpeechHues.Default : Hue));
        writer.Write((ushort)Font);
        writer.WriteAscii(Language, 4);
        writer.WriteAscii(Name, 30);
        writer.WriteBigUniNull(Text);
        writer.WritePacketLength();
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining < 2)
        {
            return false;
        }

        var declaredLength = reader.ReadUInt16();

        if (declaredLength != reader.Length || reader.Remaining < 43)
        {
            return false;
        }

        Serial = (Serial)reader.ReadUInt32();
        Graphic = reader.ReadUInt16();
        MessageType = (ChatMessageType)reader.ReadByte();
        Hue = (short)reader.ReadUInt16();
        Font = (short)reader.ReadUInt16();
        Language = reader.ReadAscii(4);
        Name = reader.ReadAscii(30).TrimEnd('\0');
        Text = reader.ReadBigUniSafe().TrimEnd('\0');

        return reader.Remaining == 0;
    }
}
