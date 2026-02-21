using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using System.Text;

namespace Moongate.Network.Packets.Incoming.UI;

[PacketHandler(0xB1, PacketSizing.Variable, Description = "Gump Menu Selection")]
public class GumpMenuSelectionPacket : BaseGameNetworkPacket
{
    public uint ButtonId { get; private set; }

    public uint GumpId { get; private set; }

    public uint Serial { get; private set; }

    public IReadOnlyList<uint> Switches { get; private set; } = [];

    public IReadOnlyDictionary<ushort, string> TextEntries { get; private set; } =
        new Dictionary<ushort, string>();

    public GumpMenuSelectionPacket()
        : base(0xB1) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining < 17)
        {
            return false;
        }

        var declaredLength = reader.ReadUInt16();
        if (declaredLength != reader.Length)
        {
            return false;
        }

        Serial = reader.ReadUInt32();
        GumpId = reader.ReadUInt32();
        ButtonId = reader.ReadUInt32();

        var switchCount = reader.ReadInt32();
        if (switchCount < 0 || reader.Remaining < switchCount * 4 + 4)
        {
            return false;
        }

        if (switchCount == 0)
        {
            Switches = [];
        }
        else
        {
            var switches = new uint[switchCount];

            for (var i = 0; i < switchCount; i++)
            {
                switches[i] = reader.ReadUInt32();
            }

            Switches = switches;
        }

        var textCount = reader.ReadInt32();
        if (textCount < 0)
        {
            return false;
        }

        var textEntries = new Dictionary<ushort, string>(textCount);
        for (var i = 0; i < textCount; i++)
        {
            if (reader.Remaining < 4)
            {
                return false;
            }

            var entryId = reader.ReadUInt16();
            var charLength = reader.ReadUInt16();
            var byteLength = checked((int)charLength * 2);

            if (reader.Remaining < byteLength)
            {
                return false;
            }

            var rawText = reader.ReadBytes(byteLength);
            var text = Encoding.BigEndianUnicode.GetString(rawText);
            textEntries[entryId] = text;
        }

        TextEntries = textEntries;

        return reader.Remaining == 0;
    }
}
