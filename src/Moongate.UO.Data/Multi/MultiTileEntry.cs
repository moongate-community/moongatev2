using Moongate.UO.Data.Tiles;
using Moongate.UO.Data.Types;

namespace Moongate.UO.Data.Multi;

public struct MultiTileEntry
{
    public ushort ItemId { get; set; }
    public short OffsetX { get; set; }
    public short OffsetY { get; set; }
    public short OffsetZ { get; set; }
    public UOTileFlag Flags { get; set; }

    public MultiTileEntry(ushort itemID, short xOffset, short yOffset, short zOffset, UOTileFlag flags)
    {
        ItemId = itemID;
        OffsetX = xOffset;
        OffsetY = yOffset;
        OffsetZ = zOffset;
        Flags = flags;
    }
}
