using System.Runtime.CompilerServices;
using Moongate.Core.Extensions.Strings;
using Moongate.UO.Data.Skills;
using Moongate.UO.Data.Types;

namespace Moongate.UO.Data.Professions;

public class ProfessionInfo
{
    public static ProfessionInfo[] Professions;

    public int ID { get; set; }
    public string Name { get; set; }
    public int NameID { get; set; }
    public int DescID { get; set; }
    public bool TopLevel { get; set; }
    public int GumpID { get; set; }

    public (UOSkillName, byte)[] Skills => _skills;

    private (UOSkillName, byte)[] _skills;

    public byte[] Stats { get; }

    public ProfessionInfo()
    {
        Name = string.Empty;

        _skills = new (UOSkillName, byte)[4];
        Stats = new byte[3];
    }

    public void FixSkills()
    {
        var index = _skills.Length - 1;

        while (index >= 0)
        {
            var skill = _skills[index];

            if (skill is not (UOSkillName.Alchemy, 0))
            {
                break;
            }

            index--;
        }

        Array.Resize(ref _skills, index + 1);
    }

    public static bool GetProfession(int profIndex, out ProfessionInfo profession)
    {
        if (!VerifyProfession(profIndex))
        {
            profession = null;

            return false;
        }

        return (profession = Professions[profIndex]) != null;
    }

    public static bool TryGetSkillName(string name, out UOSkillName skillName)
    {
        if (Enum.TryParse(name, out skillName))
        {
            return true;
        }

        var lowerName = name?.ToLowerInvariant().RemoveOrdinal(" ");

        if (!string.IsNullOrEmpty(lowerName))
        {
            foreach (var so in SkillInfo.Table)
            {
                if (lowerName == so.ProfessionSkillName.ToLowerInvariant())
                {
                    skillName = (UOSkillName)so.SkillID;

                    return true;
                }
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool VerifyProfession(int profIndex)
        => profIndex > 0 && profIndex < Professions.Length;
}
