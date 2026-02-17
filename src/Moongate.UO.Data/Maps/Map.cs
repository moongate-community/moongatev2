using Moongate.UO.Data.Tiles;
using Moongate.UO.Data.Types;

namespace Moongate.UO.Data.Maps;

public class Map : IComparable<Map>, IComparable
{
    private static readonly List<Map> _allMaps = new();

    public static int MapCount => _allMaps.Count;

    public static Map[] Maps { get; } = new Map[0x100];

    public static Map Felucca => Maps[0];
    public static Map Trammel => Maps[1];
    public static Map Ilshenar => Maps[2];
    public static Map Malas => Maps[3];
    public static Map Tokuno => Maps[4];
    public static Map TerMur => Maps[5];
    public static Map Internal => Maps[0x7F];

    public int Index { get; }
    public int MapID { get; }
    public int FileIndex { get; }
    public int Width { get; }
    public int Height { get; }
    public SeasonType Season { get; }
    public string Name { get; }
    public MapRules Rules { get; }

    private TileMatrix? _tiles;

    private Map(int index, int mapId, int fileIndex, int width, int height, SeasonType season, string name, MapRules rules)
    {
        Index = index;
        MapID = mapId;
        FileIndex = fileIndex;
        Width = width;
        Height = height;
        Season = season;
        Name = name;
        Rules = rules;

        _tiles = new(fileIndex, mapId, width, height);
    }

    public TileMatrix Tiles => _tiles ??= new(FileIndex, MapID, Width, Height);

    public int CompareTo(Map other)
        => other == null ? 1 : string.Compare(Name, other.Name, StringComparison.Ordinal);

    public int CompareTo(object obj)
        => obj is Map map ? CompareTo(map) : 1;

    public LandTile GetLandTile(int x, int y)
        => _tiles.GetLandTile(x, y);

    public static Map GetMap(int index)
    {
        if (index < 0 || index >= Maps.Length)
        {
            return null;
        }

        return Maps[index];
    }

    public static Map RegisterMap(
        int index,
        int mapID,
        int fileIndex,
        int width,
        int height,
        SeasonType season,
        string name,
        MapRules rules
    )
    {
        var m = new Map(index, mapID, fileIndex, width, height, season, name, rules);
        Maps[index] = m;
        _allMaps.Add(m);

        return m;
    }

    // public StaticTile[] GetStaticTiles(int x, int y)
    // {
    //
    //     return _tiles.GetStaticTiles(x, y);
    // }
}
