using Moongate.UO.Data.Files;
using Serilog;

namespace Moongate.UO.Data.Tiles;

public class TileMatrix
{
    public const int SectorShift = 3;

    private static readonly ILogger logger = Log.ForContext<TileMatrix>();
    private static readonly List<TileMatrix> _instances = new();

    private readonly StaticTile[][][][][] _staticTiles;
    private readonly LandTile[][][] _landTiles;
    private readonly LandTile[] _invalidLandBlock;
    private readonly UOPEntry[] _uopMapEntries;

    private readonly int _fileIndex;

    //private readonly Map _map;
    private readonly int[][] _staticPatches;
    private readonly int[][] _landPatches;
    private readonly List<TileMatrix> _fileShare = new();

    public TileMatrixPatch Patch { get; }
    public int BlockWidth { get; }
    public int BlockHeight { get; }
    public FileStream MapStream { get; }
    public FileStream IndexStream { get; }
    public FileStream DataStream { get; }
    public BinaryReader IndexReader { get; }

    public static bool Pre6000ClientSupport { get; private set; }

    //    public TileMatrix(Map owner, int fileIndex, int mapID, int width, int height)

    public TileMatrix(int fileIndex, int mapID, int width, int height)
    {
        lock (_instances)
        {
            for (var i = 0; i < _instances.Count; ++i)
            {
                var tm = _instances[i];

                if (tm._fileIndex == fileIndex)
                {
                    lock (_fileShare)
                    {
                        lock (tm._fileShare)
                        {
                            tm._fileShare.Add(this);
                            _fileShare.Add(tm);
                        }
                    }
                }
            }

            _instances.Add(this);
        }

        _fileIndex = fileIndex;
        BlockWidth = width >> SectorShift;
        BlockHeight = height >> SectorShift;

        //  _map = owner;

        if (fileIndex != 0x7F)
        {
            var mapFileIndex = Pre6000ClientSupport && mapID == 1 ? 0 : fileIndex;

            var mapPath = UoFiles.FindDataFile($"map{mapFileIndex}.mul", false);

            if (mapPath != null)
            {
                MapStream = new(mapPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            else
            {
                mapPath = UoFiles.FindDataFile($"map{mapFileIndex}LegacyMUL.uop", false);

                if (mapPath != null)
                {
                    MapStream = new(mapPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                    var uopEntries = UOPFiles.ReadUOPIndexes(MapStream, ".dat", 0x14000, 5);

                    _uopMapEntries = GC.AllocateUninitializedArray<UOPEntry>(uopEntries.Count);
                    uopEntries.Values.CopyTo(_uopMapEntries, 0);

                    ConvertToMapEntries(MapStream);
                }
                else
                {
                    logger.Warning("{File} was not found.", $"map{mapFileIndex}.mul");
                }
            }

            var indexPath = UoFiles.FindDataFile($"staidx{mapFileIndex}.mul", false);

            if (indexPath != null)
            {
                IndexStream = new(indexPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                IndexReader = new(IndexStream);
            }
            else
            {
                logger.Warning("{File} was not found.", $"staidx{mapFileIndex}.mul");
            }

            var staticsPath = UoFiles.FindDataFile($"statics{mapFileIndex}.mul", false);

            if (staticsPath != null)
            {
                DataStream = new(staticsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            else
            {
                logger.Warning("{File} was not found.", $"statics{fileIndex}.mul");
            }
        }

        EmptyStaticBlock = new StaticTile[8][][];

        for (var i = 0; i < 8; ++i)
        {
            EmptyStaticBlock[i] = new StaticTile[8][];

            for (var j = 0; j < 8; ++j)
            {
                EmptyStaticBlock[i][j] = new StaticTile[0];
            }
        }

        _invalidLandBlock = new LandTile[196];

        _landTiles = new LandTile[BlockWidth][][];
        _staticTiles = new StaticTile[BlockWidth][][][][];
        _staticPatches = new int[BlockWidth][];
        _landPatches = new int[BlockWidth][];

        Patch = new(this, fileIndex);
    }

    public StaticTile[][][] EmptyStaticBlock { get; }

    private TileList[][] m_Lists;

    private StaticTile[] m_TileBuffer = new StaticTile[128];

    private DateTime m_NextStaticWarning;
    private DateTime m_NextLandWarning;

    public static void Configure()
    {
        // Set to true to support < 6.0.0 clients where map0.mul is both Felucca & Trammel
        var isPre6000Trammel = false;
        Pre6000ClientSupport =
            isPre6000Trammel; //ServerConfiguration.GetSetting("maps.enablePre6000Trammel", isPre6000Trammel);
    }

    public void Dispose()
    {
        MapStream?.Close();
        DataStream?.Close();
        IndexReader?.Close();
    }

    public long FindOffset(long offset)
    {
        var total = 0;

        for (var i = 0; i < _uopMapEntries.Length; ++i)
        {
            var entry = _uopMapEntries[i];
            var newTotal = total + entry.Size;

            if (offset < newTotal)
            {
                return entry.Offset + (offset - total);
            }

            total = newTotal;
        }

        return -1;
    }

    public LandTile[] GetLandBlock(int x, int y)
    {
        if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight || MapStream == null)
        {
            return _invalidLandBlock;
        }

        _landTiles[x] ??= new LandTile[BlockHeight][];

        var tiles = _landTiles[x][y];

        if (tiles != null)
        {
            return tiles;
        }

        for (var i = 0; tiles == null && i < _fileShare.Count; ++i)
        {
            var shared = _fileShare[i];

            if (x < shared.BlockWidth && y < shared.BlockHeight)
            {
                var theirTiles = shared._landTiles[x];

                if (theirTiles != null)
                {
                    tiles = theirTiles[y];
                }

                if (tiles != null)
                {
                    var theirBits = shared._landPatches[x];

                    if (theirBits != null && (theirBits[y >> 5] & (1 << (y & 0x1F))) != 0)
                    {
                        tiles = null;
                    }
                }
            }
        }

        tiles ??= ReadLandBlock(x, y);

        _landTiles[x][y] = tiles;

        return tiles;
    }

    public LandTile GetLandTile(int x, int y)
    {
        var tiles = GetLandBlock(x >> SectorShift, y >> SectorShift);

        return tiles[((y & 0x7) << 3) + (x & 0x7)];
    }

    public StaticTile[][][] GetStaticBlock(int x, int y)
    {
        if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight || DataStream == null || IndexStream == null)
        {
            return EmptyStaticBlock;
        }

        _staticTiles[x] ??= new StaticTile[BlockHeight][][][];

        var tiles = _staticTiles[x][y];

        if (tiles == null)
        {
            for (var i = 0; tiles == null && i < _fileShare.Count; ++i)
            {
                var shared = _fileShare[i];

                lock (shared)
                {
                    if (x < shared.BlockWidth && y < shared.BlockHeight)
                    {
                        var theirTiles = shared._staticTiles[x];

                        if (theirTiles != null)
                        {
                            tiles = theirTiles[y];
                        }

                        if (tiles != null)
                        {
                            var theirBits = shared._staticPatches[x];

                            if (theirBits != null && (theirBits[y >> 5] & (1 << (y & 0x1F))) != 0)
                            {
                                tiles = null;
                            }
                        }
                    }
                }
            }

            tiles ??= ReadStaticBlock(x, y);

            _staticTiles[x][y] = tiles;
        }

        return tiles;
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public Map.StaticTileEnumerable GetStaticTiles(int x, int y) => new(_map, new Point2D(x, y), includeMultis: false);
    //
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public Map.StaticTileEnumerable GetStaticAndMultiTiles(int x, int y) => new(_map, new Point2D(x, y));
    //
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public Map.StaticTileEnumerable GetMultiTiles(int x, int y) => new(_map, new Point2D(x, y), false);

    public void SetLandBlock(int x, int y, LandTile[] value)
    {
        if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight)
        {
            return;
        }

        _landTiles[x] ??= new LandTile[BlockHeight][];
        _landTiles[x][y] = value;

        _landPatches[x] ??= new int[(BlockHeight + 31) >> 5];
        _landPatches[x][y >> 5] |= 1 << (y & 0x1F);
    }

    public void SetStaticBlock(int x, int y, StaticTile[][][] value)
    {
        if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight)
        {
            return;
        }

        _staticTiles[x] ??= new StaticTile[BlockHeight][][][];
        _staticTiles[x][y] = value;

        _staticPatches[x] ??= new int[(BlockHeight + 31) >> 5];
        _staticPatches[x][y >> 5] |= 1 << (y & 0x1F);
    }

    private void ConvertToMapEntries(FileStream stream)
    {
        // Sorting by offset to make seeking faster
        Array.Sort(_uopMapEntries, (a, b) => a.Offset.CompareTo(b.Offset));

        var reader = new BinaryReader(stream);

        for (var i = 0; i < _uopMapEntries.Length; ++i)
        {
            var entry = _uopMapEntries[i];
            stream.Seek(entry.Offset, SeekOrigin.Begin);
            _uopMapEntries[i].Extra = reader.ReadInt32(); // order
        }

        Array.Sort(_uopMapEntries, (a, b) => a.Extra.CompareTo(b.Extra));
    }

    private unsafe LandTile[] ReadLandBlock(int x, int y)
    {
        try
        {
            long offset = (x * BlockHeight + y) * 196 + 4;

            if (_uopMapEntries != null)
            {
                offset = FindOffset(offset);
            }

            MapStream.Seek(offset, SeekOrigin.Begin);

            var tiles = new LandTile[64];

            fixed (LandTile* pTiles = tiles)
            {
                _ = MapStream.Read(new(pTiles, 192));
            }

            return tiles;
        }
        catch (Exception ex)
        {
            if (DateTime.Now >= m_NextLandWarning)
            {
                // logger.Warning("Warning: Land EOS for {0} ({1}, {2})", _map, x, y);
                m_NextLandWarning = DateTime.Now + TimeSpan.FromMinutes(1.0);
            }

            return _invalidLandBlock;
        }
    }

    private unsafe StaticTile[][][] ReadStaticBlock(int x, int y)
    {
        try
        {
            IndexReader.BaseStream.Seek((x * BlockHeight + y) * 12, SeekOrigin.Begin);

            var lookup = IndexReader.ReadInt32();
            var length = IndexReader.ReadInt32();

            if (lookup < 0 || length <= 0)
            {
                return EmptyStaticBlock;
            }

            var count = length / 7;

            DataStream.Seek(lookup, SeekOrigin.Begin);

            if (m_TileBuffer.Length < count)
            {
                m_TileBuffer = new StaticTile[count];
            }

            var staTiles = m_TileBuffer;

            fixed (StaticTile* pTiles = staTiles)
            {
                _ = DataStream.Read(new(pTiles, length));

                if (m_Lists == null)
                {
                    m_Lists = new TileList[8][];

                    for (var i = 0; i < 8; ++i)
                    {
                        m_Lists[i] = new TileList[8];

                        for (var j = 0; j < 8; ++j)
                        {
                            m_Lists[i][j] = new();
                        }
                    }
                }

                var lists = m_Lists;

                StaticTile* pCur = pTiles,
                            pEnd = pTiles + count;

                while (pCur < pEnd)
                {
                    lists[pCur->m_X & 0x7][pCur->m_Y & 0x7].Add(pCur);

                    pCur = pCur + 1;
                }

                var tiles = new StaticTile[8][][];

                for (var i = 0; i < 8; ++i)
                {
                    tiles[i] = new StaticTile[8][];

                    for (var j = 0; j < 8; ++j)
                    {
                        tiles[i][j] = lists[i][j].ToArray();
                    }
                }

                return tiles;
            }
        }
        catch (EndOfStreamException ex)
        {
            if (DateTime.Now >= m_NextStaticWarning)
            {
                // logger.Warning("Warning: Static EOS for {0} ({1}, {2})", _map, x, y);
                m_NextStaticWarning = DateTime.Now + TimeSpan.FromMinutes(1.0);
            }

            return EmptyStaticBlock;
        }
    }
}
