using System.Text.Json.Serialization;

namespace DarkLilly.Scripting.Lua.Data.Luarc;

/// <summary>
/// Configuration class for Lua Language Server (.luarc.json file)
/// </summary>
public class LuarcConfig
{
    [JsonPropertyName("$schema")]

    /// <summary>
    /// 
    /// </summary>
    public string Schema { get; set; } = "https://raw.githubusercontent.com/sumneko/vscode-lua/master/setting/schema.json";

    /// <summary>
    /// Gets or sets the runtime configuration.
    /// </summary>
    [JsonPropertyName("runtime")]
    public LuarcRuntimeConfig Runtime { get; set; } = new();

    /// <summary>
    /// Gets or sets the workspace configuration.
    /// </summary>
    [JsonPropertyName("workspace")]
    public LuarcWorkspaceConfig Workspace { get; set; } = new();

    /// <summary>
    /// Gets or sets the diagnostics configuration.
    /// </summary>
    [JsonPropertyName("diagnostics")]
    public LuarcDiagnosticsConfig Diagnostics { get; set; } = new();

    /// <summary>
    /// Gets or sets the completion configuration.
    /// </summary>
    [JsonPropertyName("completion")]
    public LuarcCompletionConfig Completion { get; set; } = new();

    /// <summary>
    /// Gets or sets the format configuration.
    /// </summary>
    [JsonPropertyName("format")]
    public LuarcFormatConfig Format { get; set; } = new();
}
