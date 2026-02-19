using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Types;

namespace Moongate.Network.Packets.Incoming.Login;

[PacketHandler(0xF8, PacketSizing.Fixed, Length = 106, Description = "Character Creation ( 7.0.16.0 )")]
public class CharacterCreation70160Packet : BaseGameNetworkPacket
{
    public int Slot { get; private set; }
    public string CharacterName { get; private set; } = string.Empty;
    public ClientFlags ClientFlags { get; private set; }
    public int ProfessionId { get; private set; }
    public List<CharacterCreationSkillValue> Skills { get; } = [];
    public int Intelligence { get; private set; }
    public int Strength { get; private set; }
    public int Dexterity { get; private set; }
    public GenderType Gender { get; private set; }
    public int RaceIndex { get; private set; }
    public short SkinHue { get; private set; }
    public short HairStyle { get; private set; }
    public short HairHue { get; private set; }
    public short FacialHairStyle { get; private set; }
    public short FacialHairHue { get; private set; }
    public short StartingCityIndex { get; private set; }
    public short ShirtHue { get; private set; }
    public short PantsHue { get; private set; }

    public CharacterCreation70160Packet()
        : base(0xF8, 106) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        reader.ReadInt32(); // 0xEDEDEDED
        reader.ReadInt32(); // 0xFFFFFFFF
        reader.ReadByte();  // 0x00
        CharacterName = reader.ReadAscii(30);
        reader.ReadBytes(2);

        ClientFlags = (ClientFlags)reader.ReadUInt32();
        reader.ReadInt32();
        reader.ReadInt32(); // Reserved

        ProfessionId = reader.ReadByte();
        reader.ReadBytes(15);

        if (!TryParseGenderAndRace(reader.ReadByte(), out var gender, out var raceIndex))
        {
            return false;
        }

        Gender = gender;
        RaceIndex = raceIndex;

        Strength = reader.ReadByte();
        Dexterity = reader.ReadByte();
        Intelligence = reader.ReadByte();

        Skills.Clear();
        Skills.Add(new((UOSkillName)reader.ReadByte(), reader.ReadByte()));
        Skills.Add(new((UOSkillName)reader.ReadByte(), reader.ReadByte()));
        Skills.Add(new((UOSkillName)reader.ReadByte(), reader.ReadByte()));
        Skills.Add(new((UOSkillName)reader.ReadByte(), reader.ReadByte()));

        SkinHue = reader.ReadInt16();
        HairStyle = reader.ReadInt16();
        HairHue = reader.ReadInt16();
        FacialHairStyle = reader.ReadInt16();
        FacialHairHue = reader.ReadInt16();

        StartingCityIndex = reader.ReadInt16();
        reader.ReadBytes(2);
        Slot = reader.ReadInt16();

        reader.ReadInt32(); // Reserved

        ShirtHue = reader.ReadInt16();
        PantsHue = reader.ReadInt16();

        return true;
    }

    private static bool TryParseGenderAndRace(byte value, out GenderType gender, out int raceIndex)
    {
        switch (value)
        {
            case 0:
            case 2:
                gender = GenderType.Male;
                raceIndex = 0; // Human
                return true;
            case 1:
            case 3:
                gender = GenderType.Female;
                raceIndex = 0; // Human
                return true;
            case 4:
                gender = GenderType.Male;
                raceIndex = 1; // Elf
                return true;
            case 5:
                gender = GenderType.Female;
                raceIndex = 1; // Elf
                return true;
            case 6:
                gender = GenderType.Male;
                raceIndex = 2; // Gargoyle
                return true;
            case 7:
                gender = GenderType.Female;
                raceIndex = 2; // Gargoyle
                return true;
            default:
                gender = default;
                raceIndex = -1;
                return false;
        }
    }
}
