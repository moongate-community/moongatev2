using Moongate.UO.Data.Geometry;

namespace Moongate.UO.Data.Maps;

/// <summary>
/// Statistics for a single sector
/// </summary>
public record SectorStats
{
    public int MapIndex { get; init; }
    public int SectorX { get; init; }
    public int SectorY { get; init; }
    public int TotalEntities { get; init; }
    public int MobileCount { get; init; }
    public int ItemCount { get; init; }
    public int PlayerCount { get; init; }
    public Rectangle2D Bounds { get; init; }
}
