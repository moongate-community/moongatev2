using Moongate.UO.Data.Geometry;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Tiles;
using Serilog;

namespace Moongate.UO.Data.Containers;

public class ContainerLayoutSystem
{
    private readonly ILogger _logger = Log.ForContext<ContainerLayoutSystem>();

    /// <summary>
    /// Container size definitions based on UO standards
    /// </summary>
    public static readonly Dictionary<int, ContainerSize> ContainerSizes = new();

    /// <summary>
    /// Enhanced version of your original function with intelligent positioning
    /// </summary>
    public static void AddContainerItems(
        UOItemEntity container,
        List<string> containerItems,
        Func<string, Dictionary<string, object>?, UOItemEntity> createItemFunc,
        Dictionary<string, object>? overrides = null
    )
    {
        if (containerItems.Count == 0)
        {
            return;
        }

        /// Create all items first
        var itemsToAdd = containerItems
                         .Select(containerName => createItemFunc(containerName, overrides))
                         .Where(item => item != null)
                         .ToList();

        /// Use intelligent arrangement
        ArrangeItemsInContainer(container, itemsToAdd);
    }

    /// <summary>
    /// Automatically places items in a container using intelligent positioning
    /// </summary>
    public static void ArrangeItemsInContainer(UOItemEntity container, List<UOItemEntity> itemsToAdd)
    {
        var containerSize = GetContainerSize(container);
        var layout = new ContainerLayout(containerSize);

        /// Sort items by size (largest first for better packing)
        var sortedItems = itemsToAdd
                          .OrderByDescending(item => GetItemSize(item).Width * GetItemSize(item).Height)
                          .ToList();

        foreach (var item in sortedItems)
        {
            var position = layout.FindNextAvailablePosition(item);

            if (position.HasValue)
            {
                container.AddItem(item, position.Value);
                layout.MarkSpaceOccupied(position.Value, GetItemSize(item));
            }
            else
            {
                /// Container is full, place at (0,0) and let it overlap
                container.AddItem(item, new(0, 0));
            }
        }
    }

    /// <summary>
    /// Simple grid-based arrangement (alternative approach)
    /// </summary>
    public static void ArrangeItemsInGrid(UOItemEntity container, List<UOItemEntity> itemsToAdd)
    {
        var containerSize = GetContainerSize(container);
        var currentX = 0;
        var currentY = 0;

        foreach (var item in itemsToAdd)
        {
            var itemSize = GetItemSize(item);

            /// Check if item fits in current row
            if (currentX + itemSize.Width > containerSize.Width)
            {
                /// Move to next row
                currentX = 0;
                currentY++;
            }

            /// Check if we have vertical space
            if (currentY + itemSize.Height > containerSize.Height)
            {
                /// Container is full, start overlapping at (0,0)
                currentX = 0;
                currentY = 0;
            }

            container.AddItem(item, new(currentX, currentY));

            /// Move to next position
            currentX += itemSize.Width;
        }
    }

    /// <summary>
    /// Gets the size of a container based on its GumpId
    /// </summary>
    public static ContainerSize GetContainerSize(UOItemEntity container)
    {
        if (container.GumpId.HasValue && ContainerSizes.TryGetValue(container.GumpId.Value, out var size))
        {
            return size;
        }

        /// Default to standard backpack size if unknown
        return new(7, 4, "Unknown Container");
    }

    /// <summary>
    /// Gets the size an item occupies in a container
    /// Items can have different sizes based on their type
    /// </summary>
    public static Rectangle2D GetItemSize(UOItemEntity item)
    {
        var itemData = TileData.ItemTable[item.ItemId];

        return new(0, 0, itemData.CalcHeight, itemData.Height);

        // return item.ItemId switch
        // {
        //     // Weapons (usually 1x2 or 2x1)
        //     >= 0x0F43 and <= 0x0F62 => new Rectangle2D(0, 0, 2, 1), // Swords, axes
        //    // >= 0x0F45 and <= 0x0F50 => new Rectangle2D(0, 0, 1, 2), // Daggers, clubs
        //     >= 0x13B0 and <= 0x13C6 => new Rectangle2D(0, 0, 2, 1), // Bows, crossbows
        //
        //     // Armor pieces (usually 1x1 or 2x2)
        //     >= 0x1408 and <= 0x1419 => new Rectangle2D(0, 0, 2, 2), // Chest armor
        //     // >= 0x13BB and <= 0x13C0 => new Rectangle2D(0, 0, 1, 1), // Helmets
        //     >= 0x13C6 and <= 0x13CE => new Rectangle2D(0, 0, 1, 1), // Gloves, gorgets
        //
        //     // Reagents and potions (small items, 1x1)
        //     >= 0x0F78 and <= 0x0F91 => new Rectangle2D(0, 0, 1, 1), // Reagents
        //     >= 0x0F0C and <= 0x0F0F => new Rectangle2D(0, 0, 1, 1), // Potions
        //
        //     // Books and scrolls
        //     >= 0x0FEF and <= 0x0FF2 => new Rectangle2D(0, 0, 1, 1), // Books
        //     >= 0x1F2D and <= 0x1F72 => new Rectangle2D(0, 0, 1, 1), // Scrolls
        //
        //     // Large items
        //     0x0E75 or 0x0E76 => new Rectangle2D(0, 0, 2, 3), // Backpacks
        //     >= 0x0E3C and <= 0x0E3F => new Rectangle2D(0, 0, 2, 2), // Containers
        //
        //     // Gold and gems (small)
        //     0x0EED => new Rectangle2D(0, 0, 1, 1), // Gold pile
        //     >= 0x0F13 and <= 0x0F30 => new Rectangle2D(0, 0, 1, 1), // Gems
        //
        //     // Default size for unknown items
        //     _ => new Rectangle2D(0, 0, 1, 1)
        // };
    }
}
