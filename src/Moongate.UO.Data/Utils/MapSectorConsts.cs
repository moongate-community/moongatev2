namespace Moongate.UO.Data.Utils;

public class MapSectorConsts
{
    /// <summary>
    /// Size of each sector in tiles (64x64 is optimal for UO)
    /// </summary>
    public const int SectorSize = 64;

    /// <summary>
    /// Bit shift for fast division/multiplication by SectorSize
    /// 64 = 2^6, so shift by 6 bits
    /// </summary>
    public const int SectorShift = 4;

    /// <summary>
    /// Maximum view range for players (used for nearby queries)
    /// </summary>
    public const int MaxViewRange = 24;
}
