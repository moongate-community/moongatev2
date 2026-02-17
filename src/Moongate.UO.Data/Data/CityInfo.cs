using Moongate.Network.Spans;
using Moongate.UO.Data.Geometry;
using Moongate.UO.Data.Maps;

namespace Moongate.UO.Data.Packets.Data;

public sealed class CityInfo
{
    private Point3D _location;

    public CityInfo(string city, string building, int description, int x, int y, int z, Map m)
    {
        City = city;
        Building = building;
        Description = description;
        Location = new(x, y, z);
        Map = m;
    }

    public CityInfo(string city, string building, int x, int y, int z, Map m) : this(city, building, 0, x, y, z, m) { }

    public CityInfo(string city, string building, int description, int x, int y, int z) : this(
        city,
        building,
        description,
        x,
        y,
        z,
        Map.Trammel
    ) { }

    public CityInfo(string city, string building, int x, int y, int z) : this(city, building, 0, x, y, z, Map.Trammel) { }

    public string City { get; set; }

    public string Building { get; set; }

    public int Description { get; set; }

    public int X
    {
        get => _location.X;
        set => _location.X = value;
    }

    public int Y
    {
        get => _location.Y;
        set => _location.Y = value;
    }

    public int Z
    {
        get => _location.Z;
        set => _location.Z = value;
    }

    public Point3D Location
    {
        get => _location;
        set => _location = value;
    }

    public Map Map { get; set; }

    public static int Length => 89;

    public byte[] ToArray(int index)
    {
        using var packetWriter = new SpanWriter(stackalloc byte[Length], true);

        packetWriter.Write((byte)index);
        packetWriter.WriteAscii(City, 32);
        packetWriter.WriteAscii(Building, 32);
        packetWriter.Write(_location.X);
        packetWriter.Write(_location.Y);
        packetWriter.Write(_location.Z);
        packetWriter.Write(Map.Index);
        packetWriter.Write(Description);
        packetWriter.Write(0); // 0x00

        return packetWriter.Span.ToArray();
    }
}
