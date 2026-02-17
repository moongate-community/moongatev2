using Moongate.Core.Extensions.Collections;
using Moongate.Core.Utils;

namespace Moongate.UO.Data.Races.Base;

public class Elf : Race
{
    private static readonly int[] m_SkinHues =
    {
        0x0BF, 0x24D, 0x24E, 0x24F, 0x353, 0x361, 0x367, 0x374,
        0x375, 0x376, 0x381, 0x382, 0x383, 0x384, 0x385, 0x389,
        0x3DE, 0x3E5, 0x3E6, 0x3E8, 0x3E9, 0x430, 0x4A7, 0x4DE,
        0x51D, 0x53F, 0x579, 0x76B, 0x76C, 0x76D, 0x835, 0x903
    };

    private static readonly int[] m_HairHues =
    {
        0x034, 0x035, 0x036, 0x037, 0x038, 0x039, 0x058, 0x08E,
        0x08F, 0x090, 0x091, 0x092, 0x101, 0x159, 0x15A, 0x15B,
        0x15C, 0x15D, 0x15E, 0x128, 0x12F, 0x1BD, 0x1E4, 0x1F3,
        0x207, 0x211, 0x239, 0x251, 0x26C, 0x2C3, 0x2C9, 0x31D,
        0x31E, 0x31F, 0x320, 0x321, 0x322, 0x323, 0x324, 0x325,
        0x326, 0x369, 0x386, 0x387, 0x388, 0x389, 0x38A, 0x59D,
        0x6B8, 0x725, 0x853
    };

    public Elf(int raceID, int raceIndex)
        : base(raceID, raceIndex, "Elf", "Elves", 605, 606, 607, 608) { }

    public override int ClipHairHue(int hue)
    {
        for (var i = 0; i < m_HairHues.Length; i++)
        {
            if (m_HairHues[i] == hue)
            {
                return hue;
            }
        }

        return m_HairHues[0];
    }

    public override int ClipSkinHue(int hue)
    {
        for (var i = 0; i < m_SkinHues.Length; i++)
        {
            if (m_SkinHues[i] == hue)
            {
                return hue;
            }
        }

        return m_SkinHues[0];
    }

    public override int RandomFacialHair(bool female)
        => 0;

    public override int RandomHair(bool female) // Random hair doesn't include baldness
    {
        return RandomUtils.Random(8) switch
        {
            0 => 0x2FC0,                   // Long Feather
            1 => 0x2FC1,                   // Short
            2 => 0x2FC2,                   // Mullet
            3 => 0x2FCE,                   // Knob
            4 => 0x2FCF,                   // Braided
            5 => 0x2FD1,                   // Spiked
            6 => female ? 0x2FCC : 0x2FBF, // Flower or Mid-long
            _ => female ? 0x2FD0 : 0x2FCD
        };
    }

    public override int RandomHairHue()
        => m_HairHues.RandomElement();

    public override int RandomSkinHue()
        => m_SkinHues.RandomElement() | 0x8000;

    public override bool ValidateFacialHair(bool female, int itemID)
        => itemID == 0;

    public override bool ValidateHair(bool female, int itemID)
    {
        if (itemID == 0)
        {
            return true;
        }

        if (female && itemID is 0x2FCD or 0x2FBF || !female && itemID is 0x2FCC or 0x2FD0)
        {
            return false;
        }

        if (itemID is >= 0x2FBF and <= 0x2FC2)
        {
            return true;
        }

        if (itemID is >= 0x2FCC and <= 0x2FD1)
        {
            return true;
        }

        return false;
    }
}
