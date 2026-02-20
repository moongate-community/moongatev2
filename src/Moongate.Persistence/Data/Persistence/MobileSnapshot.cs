using MemoryPack;

namespace Moongate.Persistence.Data.Persistence;

/// <summary>
/// Serialized mobile state used inside world snapshots and journal payloads.
/// </summary>
[MemoryPackable]
public sealed partial class MobileSnapshot
{
    public uint Id { get; set; }

    public uint AccountId { get; set; }

    public string? Name { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public int Z { get; set; }

    public int MapId { get; set; }

    public byte Direction { get; set; }

    public bool IsPlayer { get; set; }

    public bool IsAlive { get; set; }

    public byte Gender { get; set; }

    public byte RaceIndex { get; set; }

    public int ProfessionId { get; set; }

    public short SkinHue { get; set; }

    public short HairStyle { get; set; }

    public short HairHue { get; set; }

    public short FacialHairStyle { get; set; }

    public short FacialHairHue { get; set; }

    public int Strength { get; set; }

    public int Dexterity { get; set; }

    public int Intelligence { get; set; }

    public int Hits { get; set; }

    public int Mana { get; set; }

    public int Stamina { get; set; }

    public int MaxHits { get; set; }

    public int MaxMana { get; set; }

    public int MaxStamina { get; set; }

    public int Level { get; set; }

    public long Experience { get; set; }

    public int SkillPoints { get; set; }

    public int StatPoints { get; set; }

    public int FireResistance { get; set; }

    public int ColdResistance { get; set; }

    public int PoisonResistance { get; set; }

    public int EnergyResistance { get; set; }

    public int Luck { get; set; }

    public int? BaseBodyId { get; set; }

    public uint BackpackId { get; set; }

    public byte[] EquippedLayers { get; set; } = [];

    public uint[] EquippedItemIds { get; set; } = [];

    public bool IsWarMode { get; set; }

    public bool IsHidden { get; set; }

    public bool IsFrozen { get; set; }

    public bool IsParalyzed { get; set; }

    public bool IsFlying { get; set; }

    public bool IgnoreMobiles { get; set; }

    public bool IsPoisoned { get; set; }

    public bool IsBlessed { get; set; }

    public bool IsInvulnerable { get; set; }

    public bool IsMounted { get; set; }

    public byte Notoriety { get; set; }

    public long CreatedUtcTicks { get; set; }

    public long LastLoginUtcTicks { get; set; }
}
