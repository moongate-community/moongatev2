using System.Runtime.InteropServices;

namespace Moongate.UO.Data.Tiles;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct StaticTile
{
    internal ushort m_ID;
    internal byte m_X;
    internal byte m_Y;
    internal sbyte m_Z;
    internal short m_Hue;

    public int ID => m_ID;

    public int X
    {
        get => m_X;
        set => m_X = (byte)value;
    }

    public int Y
    {
        get => m_Y;
        set => m_Y = (byte)value;
    }

    public int Z
    {
        get => m_Z;
        set => m_Z = (sbyte)value;
    }

    public int Hue
    {
        get => m_Hue;
        set => m_Hue = (short)value;
    }

    public int Height => TileData.ItemTable[m_ID & TileData.MaxItemValue].Height;

    public StaticTile(ushort id, sbyte z)
    {
        m_ID = id;
        m_Z = z;

        m_X = 0;
        m_Y = 0;
        m_Hue = 0;
    }

    public StaticTile(ushort id, byte x, byte y, sbyte z, short hue)
    {
        m_ID = id;
        m_X = x;
        m_Y = y;
        m_Z = z;
        m_Hue = hue;
    }

    public void Set(ushort id, sbyte z)
    {
        m_ID = id;
        m_Z = z;
    }

    public void Set(ushort id, byte x, byte y, sbyte z, short hue)
    {
        m_ID = id;
        m_X = x;
        m_Y = y;
        m_Z = z;
        m_Hue = hue;
    }
}
