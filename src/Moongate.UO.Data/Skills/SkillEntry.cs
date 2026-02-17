using Moongate.UO.Data.Types;

namespace Moongate.UO.Data.Skills;

public class SkillEntry
{
    public double Value { get; set; }
    public SkillInfo Skill { get; set; }

    public double Base { get; set; } = 0;
    public int Cap { get; set; }
    public UOSkillLock Lock { get; set; }
}
