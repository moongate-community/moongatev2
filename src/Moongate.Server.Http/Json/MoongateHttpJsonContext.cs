using System.Text.Json.Serialization;
using Moongate.Server.Http.Data;

namespace Moongate.Server.Http.Json;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(MoongateHttpLoginRequest))]
[JsonSerializable(typeof(MoongateHttpLoginResponse))]
public partial class MoongateHttpJsonContext : JsonSerializerContext;
