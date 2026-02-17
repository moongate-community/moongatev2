using Moongate.UO.Data.Geometry;

namespace Moongate.UO.Data.Interfaces.Entities;

/// <summary>
/// Contract for item entities.
/// </summary>
public interface IItemEntity : IPositionEntity
{
    /// <summary>
    /// Gets the item graphic identifier.
    /// </summary>
    int ItemId { get; }

    /// <summary>
    /// Gets the optional container gump identifier.
    /// </summary>
    int? GumpId { get; }

    /// <summary>
    /// Adds an item to this item when it is a container.
    /// </summary>
    void AddItem(IItemEntity item, Point2D position);
}
