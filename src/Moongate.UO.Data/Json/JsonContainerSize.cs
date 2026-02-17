using System.Text.Json.Serialization;

namespace Moongate.UO.Data.Json;

public class JsonContainerSize
{
    public int ItemId { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public string Name { get; set; }

    [JsonIgnore]
    public int TotalSlots => Width * Height;

    public override string ToString()
        => $"ItemId: {ItemId}, Width: {Width}, Height: {Height}, Name: {Name}, TotalSlots: {TotalSlots}";
}
