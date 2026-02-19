using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Expansions;
using Moongate.UO.Data.Packets.Data;
using Moongate.UO.Data.Types;

namespace Moongate.Network.Packets.Outgoing.Login;

[PacketHandler(0xA9, PacketSizing.Variable, Description = "Characters / Starting Locations")]
public class CharactersStartingLocationsPacket : BaseGameNetworkPacket
{
    public List<CityInfo> Cities { get; } = [];

    public List<CharacterEntry?> Characters { get; } = [];

    public CharactersStartingLocationsPacket()
        : base(0xA9) { }

    public void FillCharacters(IReadOnlyList<CharacterEntry>? characters = null, int size = 7)
    {
        Characters.Clear();

        if (size < 1)
        {
            size = 1;
        }

        if (characters is not null)
        {
            for (var i = 0; i < characters.Count; i++)
            {
                Characters.Add(characters[i]);
            }
        }

        while (Characters.Count < size)
        {
            Characters.Add(null);
        }
    }

    public override void Write(ref SpanWriter writer)
    {
        var highestSlot = -1;

        for (var i = Characters.Count - 1; i >= 0; i--)
        {
            if (Characters[i] is not null)
            {
                highestSlot = i;

                break;
            }
        }

        // Supported character slot counts are 1, 5, 6 or 7.
        var count = Math.Max(highestSlot + 1, 7);

        if (count is > 1 and < 5)
        {
            count = 5;
        }

        var length = 11 + (89 * Cities.Count) + (count * 60);

        writer.Write(OpCode);
        writer.Write((ushort)length);
        writer.Write((byte)count);

        for (var i = 0; i < count; i++)
        {
            var character = i < Characters.Count ? Characters[i] : null;

            if (character is null)
            {
                writer.Clear(60);

                continue;
            }

            writer.WriteAscii(character.Name, 30);
            writer.Clear(30);
        }

        writer.Write((byte)Cities.Count);

        for (var i = 0; i < Cities.Count; i++)
        {
            writer.Write(Cities[i].ToArray(i));
        }

        var flags = ExpansionInfo.Table is { Length: > 0 }
                        ? ExpansionInfo.CoreExpansion.CharacterListFlags
                        : CharacterListFlags.ExpansionEJ;

        flags |= CharacterListFlags.SixthCharacterSlot | CharacterListFlags.SeventhCharacterSlot;

        writer.Write((int)flags);
        writer.Write((short)-1);
    }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
