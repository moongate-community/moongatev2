using System.Runtime.InteropServices;

namespace Moongate.UO.Data.Tiles;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LandTile
{
    internal short m_ID;
    internal sbyte m_Z;

    public int ID => m_ID;

    public int Z
    {
        get => m_Z;
        set => m_Z = (sbyte)value;
    }

    public int Height => 0;

    public bool Ignored => m_ID is 2 or 0x1DB or >= 0x1AE and <= 0x1B5;

    public LandTile(short id, sbyte z)
    {
        m_ID = id;
        m_Z = z;
    }

    public void Set(short id, sbyte z)
    {
        m_ID = id;
        m_Z = z;
    }
}
