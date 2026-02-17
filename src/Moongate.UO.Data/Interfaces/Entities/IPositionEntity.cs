using Moongate.UO.Data.Geometry;

namespace Moongate.UO.Data.Interfaces.Entities;

/// <summary>
/// Contract for entities that have a world position.
/// </summary>
public interface IPositionEntity : IUOEntity
{
    /// <summary>
    /// Gets or sets the entity world location.
    /// </summary>
    Point3D Location { get; set; }
}
