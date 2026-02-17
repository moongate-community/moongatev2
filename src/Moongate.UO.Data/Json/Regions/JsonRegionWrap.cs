namespace Moongate.UO.Data.Json.Regions;

public class JsonRegionWrap
{
    public JsonDfnHeader Header { get; set; }
    public List<JsonRegion> Regions { get; set; } = new();
    public List<JsonMusic> MusicLists { get; set; } = new();
}
