namespace Moongate.UO.Data.Types;

/// <summary>
/// Direction types for graphical effects
/// </summary>
public enum EffectDirectionType : byte
{
    /// <summary>
    /// Effect moves from source to destination
    /// Used for projectiles, magic missiles, etc.
    /// </summary>
    SourceToTarget = 0x00,

    /// <summary>
    /// Lightning strike at source location
    /// Used for lightning bolt effects
    /// </summary>
    LightningStrike = 0x01,

    /// <summary>
    /// Effect stays at current X,Y,Z coordinates
    /// Used for stationary effects like explosions
    /// </summary>
    StayAtLocation = 0x02,

    /// <summary>
    /// Effect follows the source character
    /// Used for aura effects, buffs, etc.
    /// </summary>
    FollowCharacter = 0x03
}
