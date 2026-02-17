using Moongate.UO.Data.Geometry;

namespace Moongate.UO.Data.Containers;

/// <summary>
/// Represents the size and type of a container
/// </summary>
public record ContainerSize(int Width, int Height, string Name)
{
    public int TotalSlots => Width * Height;
    public Rectangle2D Bounds => new(0, 0, Width, Height);
}
