using System.Text.Json.Serialization;
using Moongate.UO.Data.Json.Converters;
using Moongate.UO.Data.Templates.Items;

namespace Moongate.UO.Data.Templates.Mobiles;

/// <summary>
/// Serializable definition of a mobile spawn template.
/// </summary>
public class MobileTemplateDefinition : MobileTemplateDefinitionBase
{
    [JsonConverter(typeof(Int32FlexibleJsonConverter))]
    public int Body { get; set; }

    [JsonConverter(typeof(HueSpecJsonConverter))]
    public HueSpec SkinHue { get; set; }

    [JsonConverter(typeof(HueSpecJsonConverter))]
    public HueSpec HairHue { get; set; }

    public int HairStyle { get; set; }

    public int Strength { get; set; } = 50;

    public int Dexterity { get; set; } = 50;

    public int Intelligence { get; set; } = 50;

    public int Hits { get; set; } = 100;

    public int Mana { get; set; } = 100;

    public int Stamina { get; set; } = 100;

    public string Brain { get; set; } = "None";

    public List<MobileEquipmentItemTemplate> FixedEquipment { get; set; } = [];

    public List<MobileRandomEquipmentPoolTemplate> RandomEquipment { get; set; } = [];
}
