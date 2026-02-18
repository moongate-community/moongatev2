namespace Moongate.UO.Data.Interfaces.Geometry;

/// <summary>
/// Represents a three-dimensional point.
/// </summary>
public interface IPoint3D : IPoint2D
{
    /// <summary>
    /// Gets the Z coordinate.
    /// </summary>
    int Z { get; }
}
