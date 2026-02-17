namespace Moongate.UO.Data.Utils;

public static class EffectsUtils
{
#region Magic Effects

    public const ushort Fireball = 0x36D4;

    /// Fireball projectile
    public const ushort LightningBolt = 0x36F4;

    /// Lightning bolt effect
    public const ushort EnergyBolt = 0x36E4;

    /// Energy bolt projectile
    public const ushort Heal = 0x376A;

    /// Healing sparkles
    public const ushort Curse = 0x374A;

    /// Dark curse effect
    public const ushort Bless = 0x373A;

    /// Divine blessing effect
    public const ushort Poison = 0x372A;

    /// Poison cloud
    public const ushort Explosion = 0x36B0;

    /// Explosion effect
    public const ushort Paralyze = 0x374B;

    /// Paralyze effect
    public const ushort Teleport = 0x3728;

    /// Teleport effect

#endregion

#region Combat Effects

    public const ushort Arrow = 0x1BFE;

    /// Arrow projectile
    public const ushort Bolt = 0x1BFB;

    /// Crossbow bolt
    public const ushort ThrowingDagger = 0x1C01;

    /// Throwing dagger
    public const ushort BloodSplash = 0x122A;

    /// Blood splash
    public const ushort Sparks = 0x3728;

    /// Metal on metal sparks

#endregion

#region Environmental Effects

    public const ushort Rain = 0x232C;

    /// Rain drops
    public const ushort Snow = 0x232D;

    /// Snow flakes
    public const ushort Smoke = 0x3728;

    /// Smoke effect
    public const ushort Fire = 0x3709;

    /// Fire effect
    public const ushort Steam = 0x3400;

    /// Steam/mist

#endregion

#region Special Effects

    public const ushort Sparkles = 0x373A;

    /// Generic sparkles
    public const ushort Glow = 0x37C4;

    /// Glowing effect
    public const ushort Portal = 0x3728;

    /// Portal/gate effect
    public const ushort Resurrection = 0x376A;

    /// Resurrection effect
    public const ushort Death = 0x3728; /// Death effect

#endregion
}
