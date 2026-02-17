using Moongate.UO.Data.Files;

namespace Moongate.UO.Data.Tiles;

public class Art
{
    private static readonly FileIndex m_FileIndex = new(
        "Artidx.mul",
        "Art.mul",
        "artLegacyMUL.uop",
        0x10000 /*0x13FDC*/,
        4,
        ".tga",
        0x13FDC,
        false
    );

    public static int GetIdxLength()
        => (int)(m_FileIndex.IdxLength / 12);

    public static ushort GetLegalItemID(int itemID, bool checkmaxid = true)
    {
        if (itemID < 0)
        {
            return 0;
        }

        if (checkmaxid)
        {
            var max = GetMaxItemID();

            if (itemID > max)
            {
                return 0;
            }
        }

        return (ushort)itemID;
    }

    public static int GetMaxItemID()
    {
        if (GetIdxLength() >= 0x13FDC)
        {
            return 0xFFFF;
        }

        if (GetIdxLength() == 0xC000)
        {
            return 0x7FFF;
        }

        return 0x3FFF;
    }

    public static bool IsUOAHS()
        => GetIdxLength() >= 0x13FDC;
}
