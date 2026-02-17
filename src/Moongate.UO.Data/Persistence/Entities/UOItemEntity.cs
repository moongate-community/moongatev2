using Moongate.UO.Data.Geometry;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Interfaces.Entities;

namespace Moongate.UO.Data.Persistence.Entities;

/// <summary>
/// Minimal item entity implementation used by map and container systems.
/// </summary>
public class UOItemEntity : IItemEntity
{
    private readonly List<(UOItemEntity Item, Point2D Position)> _items = new();

    public Serial Id { get; set; }

    public Point3D Location { get; set; }

    public int ItemId { get; set; }

    public int? GumpId { get; set; }

    /// <summary>
    /// Gets container child items when this item acts as a container.
    /// </summary>
    public IReadOnlyList<(UOItemEntity Item, Point2D Position)> Items => _items;

    public void AddItem(IItemEntity item, Point2D position)
    {
        if (item is UOItemEntity typedItem)
        {
            _items.Add((typedItem, position));
        }
    }

    public override string ToString()
        => $"Item(Id={Id}, ItemId=0x{ItemId:X4}, Location={Location})";
}
