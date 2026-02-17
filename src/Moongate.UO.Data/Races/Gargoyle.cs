using Moongate.Core.Extensions.Collections;
using Moongate.Core.Utils;
using Moongate.UO.Data.Races.Base;

namespace Moongate.UO.Data.Races;

public class Gargoyle : Race
{
    private static readonly int[] m_HornHues =
    {
        0x709, 0x70B, 0x70D, 0x70F, 0x711, 0x763,
        0x765, 0x768, 0x76B, 0x6F3, 0x6F1, 0x6EF,
        0x6E4, 0x6E2, 0x6E0, 0x709, 0x70B, 0x70D
    };

    public Gargoyle(int raceID, int raceIndex)
        : base(raceID, raceIndex, "Gargoyle", "Gargoyles", 666, 667, 402, 403) { }

    public override int ClipHairHue(int hue)
    {
        for (var i = 0; i < m_HornHues.Length; i++)
        {
            if (m_HornHues[i] == hue)
            {
                return hue;
            }
        }

        return m_HornHues[0];
    }

    public override int ClipSkinHue(int hue)
        => hue;

    public override int RandomFacialHair(bool female)
        => female ? 0 : RandomUtils.RandomList(0, 0x42AD, 0x42AE, 0x42AF, 0x42B0);

    public override int RandomHair(bool female)
    {
        if (RandomUtils.Random(9) == 0)
        {
            return 0;
        }

        if (!female)
        {
            return 0x4258 + RandomUtils.Random(8);
        }

        return RandomUtils.Random(9) switch
        {
            0 => 0x4261,
            1 => 0x4262,
            2 => 0x4273,
            3 => 0x4274,
            4 => 0x4275,
            5 => 0x42B0,
            6 => 0x42B1,
            7 => 0x42AA,
            8 => 0x42AB,
            _ => 0
        };
    }

    public override int RandomHairHue()
        => m_HornHues.RandomElement();

    public override int RandomSkinHue()
        => RandomUtils.Random(1755, 25) | 0x8000;

    public override bool ValidateFacialHair(bool female, int itemID)
        => !female && itemID is >= 0x42AD and <= 0x42B0;

    public override bool ValidateHair(bool female, int itemID)
    {
        if (!female)
        {
            return itemID is >= 0x4258 and <= 0x425F;
        }

        return itemID is 0x4261 or 0x4262 or >= 0x4273 and <= 0x4275 or 0x42B0 or 0x42B1 or 0x42AA or 0x42AB;
    }
}
