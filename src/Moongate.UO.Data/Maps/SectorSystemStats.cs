namespace Moongate.UO.Data.Maps;

/// <summary>
/// Statistics about the sector system performance
/// </summary>
public record SectorSystemStats
{
    public int TotalSectors { get; init; }
    public int TotalEntities { get; init; }
    public int MaxEntitiesPerSector { get; init; }
    public double AverageEntitiesPerSector { get; init; }
}
