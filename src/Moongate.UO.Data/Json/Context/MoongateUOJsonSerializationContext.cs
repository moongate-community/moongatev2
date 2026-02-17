using System.Text.Json.Serialization;
using Moongate.UO.Data.Expansions;
using Moongate.UO.Data.Json.Names;
using Moongate.UO.Data.Json.Regions;
using Moongate.UO.Data.Json.Weather;
using Moongate.UO.Data.Skills;

namespace Moongate.UO.Data.Json.Context;

[JsonSourceGenerationOptions(
     PropertyNameCaseInsensitive = true,
     UseStringEnumConverter = true
 ), JsonSerializable(typeof(SkillInfo[])),
 JsonSerializable(typeof(ExpansionInfo[])),
 JsonSerializable(typeof(JsonContainerSize[])),
 JsonSerializable(typeof(JsonNameDef[])),
 JsonSerializable(typeof(JsonRegionWrap)),
 JsonSerializable(typeof(JsonWeatherWrap)),
 JsonSerializable(typeof(JsonProfessionsRoot))]
public partial class MoongateUOJsonSerializationContext : JsonSerializerContext { }
