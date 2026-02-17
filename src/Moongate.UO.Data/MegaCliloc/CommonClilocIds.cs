namespace Moongate.UO.Data.MegaCliloc;

/// <summary>
/// Common cliloc IDs used in MegaCliloc packets
/// </summary>
public static class CommonClilocIds
{
#region Object Names and Basic Info

    /// <summary>
    /// Generic object name cliloc
    /// </summary>
    public const uint ObjectName = 1000000;

    public const uint ItemName = 1050039;

    public const uint Amount = 1062217;

    /// <summary>
    /// Weight: ~1_WEIGHT~
    /// </summary>
    public const uint Weight = 1072788;

    /// <summary>
    /// Durability ~1_val~ / ~2_val~
    /// </summary>
    public const uint Durability = 1060639;

    /// <summary>
    /// Blessed
    /// </summary>
    public const uint Blessed = 1060847;

    /// <summary>
    /// Cursed
    /// </summary>
    public const uint Cursed = 1049643;

    /// <summary>
    /// Insured
    /// </summary>
    public const uint Insured = 1060848;

#endregion

#region Weapon Properties

    /// <summary>
    /// Weapon Damage ~1_val~ - ~2_val~
    /// </summary>
    public const uint WeaponDamage = 1060403;

    /// <summary>
    /// Weapon Speed ~1_val~
    /// </summary>
    public const uint WeaponSpeed = 1060486;

    /// <summary>
    /// Hit Chance Increase ~1_val~%
    /// </summary>
    public const uint HitChanceIncrease = 1060415;

    /// <summary>
    /// Damage Increase ~1_val~%
    /// </summary>
    public const uint DamageIncrease = 1060401;

    /// <summary>
    /// Swing Speed Increase ~1_val~%
    /// </summary>
    public const uint SwingSpeedIncrease = 1060486;

#endregion

#region Armor Properties

    /// <summary>
    /// Physical Resist ~1_val~%
    /// </summary>
    public const uint PhysicalResist = 1060448;

    /// <summary>
    /// Fire Resist ~1_val~%
    /// </summary>
    public const uint FireResist = 1060447;

    /// <summary>
    /// Cold Resist ~1_val~%
    /// </summary>
    public const uint ColdResist = 1060445;

    /// <summary>
    /// Poison Resist ~1_val~%
    /// </summary>
    public const uint PoisonResist = 1060449;

    /// <summary>
    /// Energy Resist ~1_val~%
    /// </summary>
    public const uint EnergyResist = 1060446;

    /// <summary>
    /// Armor Rating: ~1_val~
    /// </summary>
    public const uint ArmorRating = 1060448;

#endregion

#region Creature Properties

    /// <summary>
    /// Hit Points ~1_val~ / ~2_val~
    /// </summary>
    public const uint HitPoints = 1060578;

    /// <summary>
    /// Mana ~1_val~ / ~2_val~
    /// </summary>
    public const uint Mana = 1060581;

    /// <summary>
    /// Stamina ~1_val~ / ~2_val~
    /// </summary>
    public const uint Stamina = 1060580;

    /// <summary>
    /// Strength ~1_val~
    /// </summary>
    public const uint Strength = 1060485;

    /// <summary>
    /// Dexterity ~1_val~
    /// </summary>
    public const uint Dexterity = 1060409;

    /// <summary>
    /// Intelligence ~1_val~
    /// </summary>
    public const uint Intelligence = 1060432;

    /// <summary>
    /// Taming Difficulty: ~1_val~
    /// </summary>
    public const uint TamingDifficulty = 1060578;

#endregion

#region Player Properties

    /// <summary>
    /// Guild: ~1_val~
    /// </summary>
    public const uint Guild = 1060622;

    /// <summary>
    /// Murderer (Red)
    /// </summary>
    public const uint Murderer = 1060848;

    /// <summary>
    /// Criminal (Gray)
    /// </summary>
    public const uint Criminal = 1060849;

    /// <summary>
    /// Karma: ~1_val~
    /// </summary>
    public const uint Karma = 1060581;

    /// <summary>
    /// Fame: ~1_val~
    /// </summary>
    public const uint Fame = 1060580;

#endregion

#region Special Properties

    /// <summary>
    /// Slayer: ~1_val~
    /// </summary>
    public const uint Slayer = 1060479;

    /// <summary>
    /// Magic Item
    /// </summary>
    public const uint MagicItem = 1060485;

    /// <summary>
    /// Spell Channeling
    /// </summary>
    public const uint SpellChanneling = 1060482;

    /// <summary>
    /// Faster Cast Recovery ~1_val~
    /// </summary>
    public const uint FasterCastRecovery = 1060412;

    /// <summary>
    /// Faster Casting ~1_val~
    /// </summary>
    public const uint FasterCasting = 1060413;

    /// <summary>
    /// Spell Damage Increase ~1_val~%
    /// </summary>
    public const uint SpellDamageIncrease = 1060483;

    /// <summary>
    /// Mana Regeneration ~1_val~
    /// </summary>
    public const uint ManaRegeneration = 1060440;

    /// <summary>
    /// Hit Point Regeneration ~1_val~
    /// </summary>
    public const uint HitPointRegeneration = 1060444;

    /// <summary>
    /// Stamina Regeneration ~1_val~
    /// </summary>
    public const uint StaminaRegeneration = 1060443;

#endregion

#region Skill Properties

    /// <summary>
    /// Skill Bonus: ~1_val~ +~2_val~
    /// </summary>
    public const uint SkillBonus = 1060451;

    /// <summary>
    /// All Skills +~1_val~
    /// </summary>
    public const uint AllSkills = 1060366;

#endregion

#region Container Properties

    /// <summary>
    /// Contents: ~1_val~ items, ~2_val~ stones
    /// </summary>
    public const uint ContainerContents = 1060445;

    /// <summary>
    /// Uses Remaining: ~1_val~
    /// </summary>
    public const uint UsesRemaining = 1060584;

#endregion

#region Book Properties

    /// <summary>
    /// Pages: ~1_val~
    /// </summary>
    public const uint BookPages = 1060581;

    /// <summary>
    /// Written by ~1_val~
    /// </summary>
    public const uint BookAuthor = 1060580;

    /// <summary>
    /// Title: ~1_val~
    /// </summary>
    public const uint BookTitle = 1060579;

#endregion
}
