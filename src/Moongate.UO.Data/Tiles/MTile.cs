using Moongate.UO.Data.Types;
namespace Moongate.UO.Data.Tiles;

public struct MTile : IComparable
{
    internal ushort m_ID;
    internal sbyte m_Z;
    internal UOTileFlag m_Flag;
    internal int m_Solver;

    public ushort ID => m_ID;

    public int Z
    {
        get => m_Z;
        set => m_Z = (sbyte)value;
    }

    public UOTileFlag Flag
    {
        get => m_Flag;
        set => m_Flag = value;
    }

    public int Solver
    {
        get => m_Solver;
        set => m_Solver = value;
    }

    public MTile(ushort id, sbyte z)
    {
        m_ID = Art.GetLegalItemID(id);
        m_Z = z;
        m_Flag = UOTileFlag.Background;
        m_Solver = 0;
    }

    public MTile(ushort id, sbyte z, UOTileFlag flag)
    {
        m_ID = Art.GetLegalItemID(id);
        m_Z = z;
        m_Flag = flag;
        m_Solver = 0;
    }

    public int CompareTo(object x)
    {
        if (x == null)
        {
            return 1;
        }

        if (!(x is MTile))
        {
            throw new ArgumentNullException();
        }

        var a = (MTile)x;

        var ourData = TileData.ItemTable[m_ID];
        var theirData = TileData.ItemTable[a.ID];

        var ourTreshold = 0;

        if (ourData.Height > 0)
        {
            ++ourTreshold;
        }

        if (!ourData.Background)
        {
            ++ourTreshold;
        }

        var ourZ = Z;
        var theirTreshold = 0;

        if (theirData.Height > 0)
        {
            ++theirTreshold;
        }

        if (!theirData.Background)
        {
            ++theirTreshold;
        }

        var theirZ = a.Z;

        ourZ += ourTreshold;
        theirZ += theirTreshold;
        var res = ourZ - theirZ;

        if (res == 0)
        {
            res = ourTreshold - theirTreshold;
        }

        if (res == 0)
        {
            res = m_Solver - a.Solver;
        }

        return res;
    }

    public void Set(ushort id, sbyte z)
    {
        m_ID = Art.GetLegalItemID(id);
        m_Z = z;
    }

    public void Set(ushort id, sbyte z, UOTileFlag flag)
    {
        m_ID = Art.GetLegalItemID(id);
        m_Z = z;
        m_Flag = flag;
    }
}
