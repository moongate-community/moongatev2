namespace Moongate.UO.Data.Types;

/// <summary>
/// Flags for Object Info packet
/// </summary>
[Flags]
public enum ObjectInfoFlags : byte
{
    /// <summary>
    /// No flags
    /// </summary>
    None = 0x00,

    /// <summary>
    /// Female character
    /// </summary>
    Female = 0x02,

    /// <summary>
    /// Poisoned
    /// </summary>
    Poisoned = 0x04,

    /// <summary>
    /// Yellow hits (healthbar gets yellow)
    /// </summary>
    YellowHits = 0x08,

    /// <summary>
    /// Faction ship (unsure why client needs to know)
    /// </summary>
    FactionShip = 0x10,

    /// <summary>
    /// Movable if normally not
    /// </summary>
    Movable = 0x20,

    /// <summary>
    /// War mode
    /// </summary>
    WarMode = 0x40,

    /// <summary>
    /// Hidden
    /// </summary>
    Hidden = 0x80
}
