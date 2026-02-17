using System.Runtime.InteropServices;

namespace Moongate.UO.Data.Tiles;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Tile : IComparable
{
    internal ushort m_ID;
    internal sbyte m_Z;

    public ushort ID => m_ID;

    public int Z
    {
        get => m_Z;
        set => m_Z = (sbyte)value;
    }

    public Tile(ushort id, sbyte z)
    {
        m_ID = id;
        m_Z = z;
    }

    public Tile(ushort id, sbyte z, sbyte flag)
    {
        m_ID = id;
        m_Z = z;
    }

    public int CompareTo(object x)
    {
        if (x == null)
        {
            return 1;
        }

        if (!(x is Tile))
        {
            throw new ArgumentNullException();
        }

        var a = (Tile)x;

        if (m_Z > a.m_Z)
        {
            return 1;
        }

        if (a.m_Z > m_Z)
        {
            return -1;
        }

        var ourData = TileData.ItemTable[m_ID];
        var theirData = TileData.ItemTable[a.m_ID];

        if (ourData.Height > theirData.Height)
        {
            return 1;
        }

        if (theirData.Height > ourData.Height)
        {
            return -1;
        }

        if (ourData.Background && !theirData.Background)
        {
            return -1;
        }

        if (theirData.Background && !ourData.Background)
        {
            return 1;
        }

        return 0;
    }

    public void Set(ushort id, sbyte z)
    {
        m_ID = id;
        m_Z = z;
    }

    public void Set(ushort id, sbyte z, sbyte flag)
    {
        m_ID = id;
        m_Z = z;
    }
}
