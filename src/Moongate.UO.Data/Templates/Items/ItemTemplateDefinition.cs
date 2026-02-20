using System.Text.Json.Serialization;
using Moongate.UO.Data.Json.Converters;
using Moongate.UO.Data.Types;

namespace Moongate.UO.Data.Templates.Items;

public class ItemTemplateDefinition : ItemTemplateDefinitionBase
{
    public List<string> Container { get; set; } = [];

    public string Description { get; set; }

    public bool Dyeable { get; set; }

    [JsonConverter(typeof(GoldValueSpecJsonConverter))]
    public GoldValueSpec GoldValue { get; set; }

    public string? GumpId { get; set; }

    [JsonConverter(typeof(HueSpecJsonConverter))]
    public HueSpec Hue { get; set; }

    public bool IsMovable { get; set; }

    public string ItemId { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter<LootType>))]
    public LootType LootType { get; set; }

    public string ScriptId { get; set; }

    public bool Stackable { get; set; }

    public List<string> Tags { get; set; } = [];

    public decimal Weight { get; set; }
}
