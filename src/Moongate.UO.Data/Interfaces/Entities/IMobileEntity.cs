using Moongate.UO.Data.Types;

namespace Moongate.UO.Data.Interfaces.Entities;

/// <summary>
/// Contract for mobile entities (players and creatures).
/// </summary>
public interface IMobileEntity : IPositionEntity
{
    /// <summary>
    /// Gets whether this mobile is player-controlled.
    /// </summary>
    bool IsPlayer { get; }

    /// <summary>
    /// Gets whether this mobile is alive.
    /// </summary>
    bool IsAlive { get; }

    /// <summary>
    /// Gets the mobile gender.
    /// </summary>
    GenderType Gender { get; }
}
