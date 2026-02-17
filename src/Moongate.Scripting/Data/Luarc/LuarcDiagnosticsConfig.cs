using System.Text.Json.Serialization;

namespace DarkLilly.Scripting.Lua.Data.Luarc;

/// <summary>
/// Diagnostics configuration for Lua Language Server
/// </summary>
public class LuarcDiagnosticsConfig
{
    [JsonPropertyName("globals")]
    public string[] Globals { get; set; } = [];
}
