using Moongate.UO.Data.Types;

namespace Moongate.UO.Data.Tiles;

public struct LandData
{
    public string Name { get; set; }

    public UOTileFlag Flags { get; set; }

    public LandData(string name, UOTileFlag flags)
    {
        Name = name;
        Flags = flags;
    }

    public override string ToString()
        => $" {Name} ({Flags})";
}
