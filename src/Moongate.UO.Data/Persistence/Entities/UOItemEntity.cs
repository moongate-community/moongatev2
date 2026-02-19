using Moongate.UO.Data.Geometry;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Interfaces.Entities;
using Moongate.UO.Data.Types;

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
    /// Gets or sets parent container serial when the item is inside a container.
    /// </summary>
    public Serial ParentContainerId { get; set; }

    /// <summary>
    /// Gets or sets item position inside the parent container.
    /// </summary>
    public Point2D ContainerPosition { get; set; }

    /// <summary>
    /// Gets or sets the mobile serial when the item is equipped.
    /// </summary>
    public Serial EquippedMobileId { get; set; }

    /// <summary>
    /// Gets or sets the equipped layer when the item is worn.
    /// </summary>
    public ItemLayerType? EquippedLayer { get; set; }

    /// <summary>
    /// Gets container child items when this item acts as a container.
    /// </summary>
    public IReadOnlyList<(UOItemEntity Item, Point2D Position)> Items => _items;

    public void AddItem(IItemEntity item, Point2D position)
    {
        if (item is UOItemEntity typedItem)
        {
            typedItem.ParentContainerId = Id;
            typedItem.ContainerPosition = position;
            typedItem.EquippedMobileId = Serial.Zero;
            typedItem.EquippedLayer = null;
            _items.Add((typedItem, position));
        }
    }

    public override string ToString()
        => $"Item(Id={Id}, ItemId=0x{ItemId:X4}, Location={Location})";
}
