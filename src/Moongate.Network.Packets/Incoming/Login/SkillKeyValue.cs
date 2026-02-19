using Moongate.UO.Data.Types;

namespace Moongate.Network.Packets.Incoming.Login;

public readonly record struct SkillKeyValue(UOSkillName Skill, int Value);
