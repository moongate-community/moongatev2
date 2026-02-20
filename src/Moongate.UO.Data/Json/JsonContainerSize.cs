using System.Text.Json.Serialization;

namespace Moongate.UO.Data.Json;

public class JsonContainerSize
{
    public string Id { get; set; }

    public int ItemId { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public string Name { get; set; }

    [JsonIgnore]
    public int TotalSlots => Width * Height;

    public override string ToString()
        => $"Id: {Id}, ItemId: {ItemId}, Width: {Width}, Height: {Height}, Name: {Name}, TotalSlots: {TotalSlots}";
}
