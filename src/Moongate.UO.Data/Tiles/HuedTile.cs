using System.Runtime.InteropServices;

namespace Moongate.UO.Data.Tiles;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct HuedTile
{
    internal sbyte m_Z;
    internal ushort m_ID;
    internal int m_Hue;

    public ushort ID
    {
        get => m_ID;
        set => m_ID = value;
    }

    public int Hue
    {
        get => m_Hue;
        set => m_Hue = value;
    }

    public int Z
    {
        get => m_Z;
        set => m_Z = (sbyte)value;
    }

    public HuedTile(ushort id, short hue, sbyte z)
    {
        m_ID = id;
        m_Hue = hue;
        m_Z = z;
    }

    public void Set(ushort id, short hue, sbyte z)
    {
        m_ID = id;
        m_Hue = hue;
        m_Z = z;
    }
}
