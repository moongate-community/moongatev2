using System.Text.Json.Serialization;
using Moongate.Core.Extensions.Strings;
using Moongate.UO.Data.Types;

namespace Moongate.UO.Data.Skills;

public class SkillInfo
{
    [JsonConstructor]
    public SkillInfo(
        int skillID,
        string name,
        double strScale,
        double dexScale,
        double intScale,
        string title,
        double strGain,
        double dexGain,
        double intGain,
        double gainFactor,
        string professionSkillName,
        Stat primaryStat,
        Stat secondaryStat
    )
    {
        Name = name;
        Title = title;
        SkillID = skillID;
        StrScale = strScale / 100.0;
        DexScale = dexScale / 100.0;
        IntScale = intScale / 100.0;
        StrGain = strGain;
        DexGain = dexGain;
        IntGain = intGain;
        GainFactor = gainFactor;
        ProfessionSkillName = professionSkillName ?? Name.RemoveOrdinal(" ");
        StatTotal = strScale + dexScale + intScale;
        PrimaryStat = primaryStat;
        SecondaryStat = secondaryStat;
    }

    public int SkillID { get; }

    public string Name { get; set; }

    public string Title { get; set; }

    public double StrScale { get; set; }

    public double DexScale { get; set; }

    public double IntScale { get; set; }

    public double StatTotal { get; set; }

    public double StrGain { get; set; }

    public double DexGain { get; set; }

    public double IntGain { get; set; }

    public double GainFactor { get; set; }

    public string ProfessionSkillName { get; set; }

    public Stat PrimaryStat { get; set; }

    public Stat SecondaryStat { get; set; }

    public static SkillInfo[] Table { get; set; } = [];
}
