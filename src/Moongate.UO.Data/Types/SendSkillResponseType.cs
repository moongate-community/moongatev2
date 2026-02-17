namespace Moongate.UO.Data.Types;

public enum SendSkillResponseType : byte
{
    FullSkillList = 0x00,
    SingleSkillUpdate = 0xFF,
    FullSkillListWithCap = 0x02,
    SingleSkillUpdateWithCap = 0xDF
}
