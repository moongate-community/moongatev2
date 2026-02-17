namespace Moongate.UO.Data.Json.Regions;

public class JsonRegion
{
    public int Id { get; set; }
    public string Appearance { get; set; }
    public string Name { get; set; }
    public int MusicList { get; set; }
    public bool Guarded { get; set; }
    public string GuardList { get; set; }
    public string GuardOwner { get; set; }
    public bool MagicDamage { get; set; }
    public bool Mark { get; set; }
    public bool Escorts { get; set; }
    public bool Recall { get; set; }

    public List<JsonCoordinate> Coordinates { get; set; } = new();
}
