using System.Text.Json.Serialization;

namespace DarkLilly.Scripting.Lua.Data.Luarc;

/// <summary>
/// Completion configuration for Lua Language Server
/// </summary>
public class LuarcCompletionConfig
{
    /// <summary>
    /// Gets or sets whether completion is enabled.
    /// </summary>
    [JsonPropertyName("enable")]
    public bool Enable { get; set; } = true;

    /// <summary>
    /// Gets or sets the call snippet setting.
    /// </summary>
    [JsonPropertyName("callSnippet")]
    public string CallSnippet { get; set; } = "Replace";
}
