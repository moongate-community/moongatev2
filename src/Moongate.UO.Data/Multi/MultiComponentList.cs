using Moongate.UO.Data.Geometry;
using Moongate.UO.Data.Tiles;
using Moongate.UO.Data.Types;

namespace Moongate.UO.Data.Multi;

public sealed class MultiComponentList
{
    public static readonly MultiComponentList Empty = new();

    private Point2D m_Min,
                    m_Max;

    public MultiComponentList(MultiComponentList toCopy)
    {
        m_Min = toCopy.m_Min;
        m_Max = toCopy.m_Max;

        Center = toCopy.Center;

        Width = toCopy.Width;
        Height = toCopy.Height;

        Tiles = new StaticTile[Width][][];

        for (var x = 0; x < Width; ++x)
        {
            Tiles[x] = new StaticTile[Height][];

            for (var y = 0; y < Height; ++y)
            {
                Tiles[x][y] = new StaticTile[toCopy.Tiles[x][y].Length];

                for (var i = 0; i < Tiles[x][y].Length; ++i)
                {
                    Tiles[x][y][i] = toCopy.Tiles[x][y][i];
                }
            }
        }

        List = new MultiTileEntry[toCopy.List.Length];

        for (var i = 0; i < List.Length; ++i)
        {
            List[i] = toCopy.List[i];
        }
    }

    // public MultiComponentList(IGenericReader reader)
    // {
    //     var version = reader.ReadInt();
    //
    //     m_Min = reader.ReadPoint2D();
    //     m_Max = reader.ReadPoint2D();
    //     Center = reader.ReadPoint2D();
    //     Width = reader.ReadInt();
    //     Height = reader.ReadInt();
    //
    //     var length = reader.ReadInt();
    //
    //     var allTiles = List = new MultiTileEntry[length];
    //
    //     if (version == 0)
    //     {
    //         for (var i = 0; i < length; ++i)
    //         {
    //             int id = reader.ReadShort();
    //             if (id >= 0x4000)
    //             {
    //                 id -= 0x4000;
    //             }
    //
    //             allTiles[i].ItemId = (ushort)id;
    //             allTiles[i].OffsetX = reader.ReadShort();
    //             allTiles[i].OffsetY = reader.ReadShort();
    //             allTiles[i].OffsetZ = reader.ReadShort();
    //             allTiles[i].Flags = (UOTileFlag)reader.ReadInt();
    //         }
    //     }
    //     else
    //     {
    //         for (var i = 0; i < length; ++i)
    //         {
    //             allTiles[i].ItemId = reader.ReadUShort();
    //             allTiles[i].OffsetX = reader.ReadShort();
    //             allTiles[i].OffsetY = reader.ReadShort();
    //             allTiles[i].OffsetZ = reader.ReadShort();
    //             allTiles[i].Flags = (UOTileFlag)reader.ReadInt();
    //         }
    //     }
    //
    //     var tiles = new TileList[Width][];
    //     Tiles = new StaticTile[Width][][];
    //
    //     for (var x = 0; x < Width; ++x)
    //     {
    //         tiles[x] = new TileList[Height];
    //         Tiles[x] = new StaticTile[Height][];
    //
    //         for (var y = 0; y < Height; ++y)
    //         {
    //             tiles[x][y] = new TileList();
    //         }
    //     }
    //
    //     for (var i = 0; i < allTiles.Length; ++i)
    //     {
    //         if (i == 0 || allTiles[i].Flags != 0)
    //         {
    //             var xOffset = allTiles[i].OffsetX + Center.X;
    //             var yOffset = allTiles[i].OffsetY + Center.Y;
    //
    //             tiles[xOffset][yOffset].Add(allTiles[i].ItemId, (sbyte)allTiles[i].OffsetZ);
    //         }
    //     }
    //
    //     for (var x = 0; x < Width; ++x)
    //     {
    //         for (var y = 0; y < Height; ++y)
    //         {
    //             Tiles[x][y] = tiles[x][y].ToArray();
    //         }
    //     }
    // }

    public MultiComponentList(BinaryReader reader, int length, bool postHSFormat)
    {
        var count = length / (postHSFormat ? 16 : 12);
        var allTiles = List = new MultiTileEntry[count];

        for (var i = 0; i < count; ++i)
        {
            allTiles[i].ItemId = reader.ReadUInt16();
            allTiles[i].OffsetX = reader.ReadInt16();
            allTiles[i].OffsetY = reader.ReadInt16();
            allTiles[i].OffsetZ = reader.ReadInt16();
            allTiles[i].Flags = postHSFormat ? (UOTileFlag)reader.ReadUInt64() : (UOTileFlag)reader.ReadUInt32();

            var e = allTiles[i];

            if (i == 0 || e.Flags != 0)
            {
                if (e.OffsetX < m_Min.X)
                {
                    m_Min.X = e.OffsetX;
                }

                if (e.OffsetY < m_Min.Y)
                {
                    m_Min.Y = e.OffsetY;
                }

                if (e.OffsetX > m_Max.X)
                {
                    m_Max.X = e.OffsetX;
                }

                if (e.OffsetY > m_Max.Y)
                {
                    m_Max.Y = e.OffsetY;
                }
            }
        }

        Center = new(-m_Min.X, -m_Min.Y);
        Width = m_Max.X - m_Min.X + 1;
        Height = m_Max.Y - m_Min.Y + 1;

        var tiles = new TileList[Width][];
        Tiles = new StaticTile[Width][][];

        for (var i = 0; i < allTiles.Length; ++i)
        {
            if (i == 0 || allTiles[i].Flags != 0)
            {
                var xOffset = allTiles[i].OffsetX + Center.X;
                var yOffset = allTiles[i].OffsetY + Center.Y;

                tiles[xOffset] ??= new TileList[Height];
                Tiles[xOffset] ??= new StaticTile[Height][];

                tiles[xOffset][yOffset] ??= new();
                tiles[xOffset][yOffset].Add(allTiles[i].ItemId, (sbyte)allTiles[i].OffsetZ);
            }
        }

        for (var x = 0; x < Width; ++x)
        {
            Tiles[x] ??= new StaticTile[Height][];

            for (var y = 0; y < Height; ++y)
            {
                var tileList = tiles[x]?[y];
                Tiles[x][y] = tileList?.ToArray() ?? Array.Empty<StaticTile>();
            }
        }
    }

    public MultiComponentList(List<MultiTileEntry> list)
    {
        var allTiles = List = new MultiTileEntry[list.Count];

        for (var i = 0; i < list.Count; ++i)
        {
            allTiles[i].ItemId = list[i].ItemId;
            allTiles[i].OffsetX = list[i].OffsetX;
            allTiles[i].OffsetY = list[i].OffsetY;
            allTiles[i].OffsetZ = list[i].OffsetZ;

            allTiles[i].Flags = list[i].Flags;

            var e = allTiles[i];

            if (i == 0 || e.Flags != 0)
            {
                if (e.OffsetX < m_Min.X)
                {
                    m_Min.X = e.OffsetX;
                }

                if (e.OffsetY < m_Min.Y)
                {
                    m_Min.Y = e.OffsetY;
                }

                if (e.OffsetX > m_Max.X)
                {
                    m_Max.X = e.OffsetX;
                }

                if (e.OffsetY > m_Max.Y)
                {
                    m_Max.Y = e.OffsetY;
                }
            }
        }

        Center = new(-m_Min.X, -m_Min.Y);
        Width = m_Max.X - m_Min.X + 1;
        Height = m_Max.Y - m_Min.Y + 1;

        var tiles = new TileList[Width][];
        Tiles = new StaticTile[Width][][];

        for (var x = 0; x < Width; ++x)
        {
            tiles[x] = new TileList[Height];
            Tiles[x] = new StaticTile[Height][];

            for (var y = 0; y < Height; ++y)
            {
                tiles[x][y] = new();
            }
        }

        for (var i = 0; i < allTiles.Length; ++i)
        {
            if (i == 0 || allTiles[i].Flags != 0)
            {
                var xOffset = allTiles[i].OffsetX + Center.X;
                var yOffset = allTiles[i].OffsetY + Center.Y;
                var itemID = (allTiles[i].ItemId & TileData.MaxItemValue) | 0x10000;

                tiles[xOffset][yOffset].Add((ushort)itemID, (sbyte)allTiles[i].OffsetZ);
            }
        }

        for (var x = 0; x < Width; ++x)
        {
            for (var y = 0; y < Height; ++y)
            {
                Tiles[x][y] = tiles[x][y].ToArray();
            }
        }
    }

    private MultiComponentList()
    {
        Tiles = Array.Empty<StaticTile[][]>();
        List = Array.Empty<MultiTileEntry>();
    }

    public Point2D Min => m_Min;
    public Point2D Max => m_Max;

    public Point2D Center { get; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public StaticTile[][][] Tiles { get; private set; }

    public MultiTileEntry[] List { get; private set; }

    public void Add(int itemID, int x, int y, int z)
    {
        var vx = x + Center.X;
        var vy = y + Center.Y;

        if (vx >= 0 && vx < Width && vy >= 0 && vy < Height)
        {
            var oldTiles = Tiles[vx][vy];

            for (var i = oldTiles.Length - 1; i >= 0; --i)
            {
                var data = TileData.ItemTable[itemID & TileData.MaxItemValue];

                if (oldTiles[i].Z == z && oldTiles[i].Height > 0 == data.Height > 0)
                {
                    var newIsRoof = (data.Flags & UOTileFlag.Roof) != 0;
                    var oldIsRoof =
                        (TileData.ItemTable[oldTiles[i].ID & TileData.MaxItemValue].Flags & UOTileFlag.Roof) != 0;

                    if (newIsRoof == oldIsRoof)
                    {
                        Remove(oldTiles[i].ID, x, y, z);
                    }
                }
            }

            oldTiles = Tiles[vx][vy];

            var newTiles = new StaticTile[oldTiles.Length + 1];
            Array.Copy(oldTiles, newTiles, oldTiles.Length);

            newTiles[^1] = new((ushort)itemID, (sbyte)z);

            Tiles[vx][vy] = newTiles;

            var oldList = List;
            var newList = new MultiTileEntry[oldList.Length + 1];

            for (var i = 0; i < oldList.Length; ++i)
            {
                newList[i] = oldList[i];
            }

            newList[oldList.Length] = new(
                (ushort)itemID,
                (short)x,
                (short)y,
                (short)z,
                UOTileFlag.Background
            );

            List = newList;

            if (x < m_Min.X)
            {
                m_Min.X = x;
            }

            if (y < m_Min.Y)
            {
                m_Min.Y = y;
            }

            if (x > m_Max.X)
            {
                m_Max.X = x;
            }

            if (y > m_Max.Y)
            {
                m_Max.Y = y;
            }
        }
    }

    public void Remove(int itemID, int x, int y, int z)
    {
        var vx = x + Center.X;
        var vy = y + Center.Y;

        if (vx >= 0 && vx < Width && vy >= 0 && vy < Height)
        {
            var oldTiles = Tiles[vx][vy];

            for (var i = 0; i < oldTiles.Length; ++i)
            {
                var tile = oldTiles[i];

                if (tile.ID == itemID && tile.Z == z)
                {
                    var newTiles = new StaticTile[oldTiles.Length - 1];
                    Array.Copy(oldTiles, newTiles, i);
                    Array.Copy(oldTiles, i + 1, newTiles, i, oldTiles.Length - i - 1);

                    Tiles[vx][vy] = newTiles;

                    break;
                }
            }

            var oldList = List;

            for (var i = 0; i < oldList.Length; ++i)
            {
                var tile = oldList[i];

                if (tile.ItemId == itemID &&
                    tile.OffsetX == (short)x &&
                    tile.OffsetY == (short)y &&
                    tile.OffsetZ == (short)z)
                {
                    var newList = new MultiTileEntry[oldList.Length - 1];
                    Array.Copy(oldList, newList, i);
                    Array.Copy(oldList, i + 1, newList, i, oldList.Length - i - 1);

                    List = newList;

                    break;
                }
            }
        }
    }

    public void RemoveXYZH(int x, int y, int z, int minHeight)
    {
        var vx = x + Center.X;
        var vy = y + Center.Y;

        if (vx >= 0 && vx < Width && vy >= 0 && vy < Height)
        {
            var oldTiles = Tiles[vx][vy];

            for (var i = 0; i < oldTiles.Length; ++i)
            {
                var tile = oldTiles[i];

                if (tile.Z == z && tile.Height >= minHeight)
                {
                    var newTiles = new StaticTile[oldTiles.Length - 1];
                    Array.Copy(oldTiles, newTiles, i);
                    Array.Copy(oldTiles, i + 1, newTiles, i, oldTiles.Length - i - 1);

                    Tiles[vx][vy] = newTiles;

                    break;
                }
            }

            var oldList = List;

            for (var i = 0; i < oldList.Length; ++i)
            {
                var tile = oldList[i];

                if (tile.OffsetX == (short)x &&
                    tile.OffsetY == (short)y &&
                    tile.OffsetZ == (short)z &&
                    TileData.ItemTable[tile.ItemId & TileData.MaxItemValue].Height >= minHeight)
                {
                    var newList = new MultiTileEntry[oldList.Length - 1];
                    Array.Copy(oldList, newList, i);
                    Array.Copy(oldList, i + 1, newList, i, oldList.Length - i - 1);

                    List = newList;

                    break;
                }
            }
        }
    }

    public void Resize(int newWidth, int newHeight)
    {
        int oldWidth = Width,
            oldHeight = Height;
        var oldTiles = Tiles;

        var totalLength = 0;

        var newTiles = new StaticTile[newWidth][][];

        for (var x = 0; x < newWidth; ++x)
        {
            newTiles[x] = new StaticTile[newHeight][];

            for (var y = 0; y < newHeight; ++y)
            {
                if (x < oldWidth && y < oldHeight)
                {
                    newTiles[x][y] = oldTiles[x][y];
                }
                else
                {
                    newTiles[x][y] = Array.Empty<StaticTile>();
                }

                totalLength += newTiles[x][y].Length;
            }
        }

        Tiles = newTiles;
        List = new MultiTileEntry[totalLength];
        Width = newWidth;
        Height = newHeight;

        m_Min = Point2D.Zero;
        m_Max = Point2D.Zero;

        var index = 0;

        for (var x = 0; x < newWidth; ++x)
        {
            for (var y = 0; y < newHeight; ++y)
            {
                var tiles = newTiles[x][y];

                for (var i = 0; i < tiles.Length; ++i)
                {
                    var tile = tiles[i];

                    var vx = x - Center.X;
                    var vy = y - Center.Y;

                    if (vx < m_Min.X)
                    {
                        m_Min.X = vx;
                    }

                    if (vy < m_Min.Y)
                    {
                        m_Min.Y = vy;
                    }

                    if (vx > m_Max.X)
                    {
                        m_Max.X = vx;
                    }

                    if (vy > m_Max.Y)
                    {
                        m_Max.Y = vy;
                    }

                    List[index++] = new(
                        (ushort)tile.ID,
                        (short)vx,
                        (short)vy,
                        (short)tile.Z,
                        UOTileFlag.Background
                    );
                }
            }
        }
    }

    // public void Serialize(IGenericWriter writer)
    // {
    //     writer.Write(1); // version;
    //
    //     writer.Write(m_Min);
    //     writer.Write(m_Max);
    //     writer.Write(Center);
    //
    //     writer.Write(Width);
    //     writer.Write(Height);
    //
    //     writer.Write(List.Length);
    //
    //     for (var i = 0; i < List.Length; ++i)
    //     {
    //         var ent = List[i];
    //
    //         writer.Write(ent.ItemId);
    //         writer.Write(ent.OffsetX);
    //         writer.Write(ent.OffsetY);
    //         writer.Write(ent.OffsetZ);
    //         writer.Write((int)ent.Flags);
    //     }
    // }
}
