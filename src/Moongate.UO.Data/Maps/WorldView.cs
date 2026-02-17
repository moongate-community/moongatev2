using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.UO.Data.Maps;

/// <summary>
/// Represents everything visible to a player
/// </summary>
public record WorldView
{
    public UOMobileEntity Player { get; init; } = null!;
    public List<UOMobileEntity> NearbyMobiles { get; init; } = new();
    public List<UOItemEntity> NearbyItems { get; init; } = new();

    public int ViewRange { get; init; }
    public int MapIndex { get; init; }

    public override string ToString()
        => $"WorldView: Player={Player}, NearbyMobilesCount={NearbyMobiles.Count}, NearbyItemsCount={NearbyItems.Count}, ViewRange={ViewRange}, MapIndex={MapIndex}";
}
