using System.Text.Json.Serialization;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Templates.Mobiles;

namespace Moongate.UO.Data.Json.Context;

[JsonSourceGenerationOptions(
     PropertyNameCaseInsensitive = true,
     UseStringEnumConverter = true
 ), JsonSerializable(typeof(ItemTemplateDefinitionBase[])),
 JsonSerializable(typeof(ItemTemplateDefinition[])),
 JsonSerializable(typeof(HueSpec)),
 JsonSerializable(typeof(GoldValueSpec)),
 JsonSerializable(typeof(MobileTemplateDefinitionBase[])),
 JsonSerializable(typeof(MobileTemplateDefinition[])),
 JsonSerializable(typeof(MobileEquipmentItemTemplate[])),
 JsonSerializable(typeof(MobileRandomEquipmentPoolTemplate[])),
 JsonSerializable(typeof(MobileWeightedEquipmentItemTemplate[]))]
public partial class MoongateUOTemplateJsonContext : JsonSerializerContext { }
