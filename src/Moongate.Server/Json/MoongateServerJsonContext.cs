using System.Text.Json.Serialization;
using Moongate.Server.Data.Config;

namespace Moongate.Server.Json;

[JsonSourceGenerationOptions(
     PropertyNameCaseInsensitive = true,
     UseStringEnumConverter = true,
     WriteIndented = true
 ), JsonSerializable(typeof(MoongateConfig)), JsonSerializable(typeof(MoongateHttpConfig))]
public partial class MoongateServerJsonContext : JsonSerializerContext { }
