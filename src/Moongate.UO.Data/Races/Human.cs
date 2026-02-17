using Moongate.Core.Utils;
using Moongate.UO.Data.Races.Base;

namespace Moongate.UO.Data.Races;

public class Human : Race
{
    public Human(int raceID, int raceIndex)
        : base(raceID, raceIndex, "Human", "Humans", 400, 401, 402, 403) { }

    public override int ClipHairHue(int hue)
    {
        return hue switch
        {
            < 1102 => 1102,
            > 1149 => 1149,
            _      => hue
        };
    }

    public override int ClipSkinHue(int hue)
    {
        return hue switch
        {
            < 1002 => 1002,
            > 1058 => 1058,
            _      => hue
        };
    }

    public override int RandomFacialHair(bool female)
    {
        if (female)
        {
            return 0;
        }

        var rand = RandomUtils.Random(7);

        return (rand < 4 ? 0x203E : 0x2047) + rand;
    }

    public override int RandomHair(bool female) // Random hair doesn't include baldness
    {
        return RandomUtils.Random(9) switch
        {
            0 => 0x203B, // Short
            1 => 0x203C, // Long
            2 => 0x203D, // Pony Tail
            3 => 0x2044, // Mohawk
            4 => 0x2045, // Pageboy
            5 => 0x2047, // Afro
            6 => 0x2049, // Pig tails
            7 => 0x204A, // Krisna
            _ => female ? 0x2046 : 0x2048
        };
    }

    public override int RandomHairHue()
        => RandomUtils.Random(1102, 48);

    public override int RandomSkinHue()
        => RandomUtils.Random(1002, 57) | 0x8000;

    public override bool ValidateFacialHair(bool female, int itemID)
    {
        if (itemID == 0)
        {
            return true;
        }

        if (female)
        {
            return false;
        }

        if (itemID is >= 0x203E and <= 0x2041)
        {
            return true;
        }

        if (itemID is >= 0x204B and <= 0x204D)
        {
            return true;
        }

        return false;
    }

    public override bool ValidateHair(bool female, int itemID)
    {
        if (itemID == 0)
        {
            return true;
        }

        if (female && itemID == 0x2048 || !female && itemID == 0x2046)
        {
            return false; // Buns & Receding Hair
        }

        if (itemID is >= 0x203B and <= 0x203D)
        {
            return true;
        }

        if (itemID is >= 0x2044 and <= 0x204A)
        {
            return true;
        }

        return false;
    }
}
