using System.Text.Json;

namespace Moongate.UO.Data.Templates.Startup;

/// <summary>
/// Represents a startup template JSON document loaded from disk.
/// </summary>
public sealed class StartupTemplateDocument
{
    /// <summary>
    /// Template identifier, usually the source file name without extension.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Raw JSON payload for the template.
    /// </summary>
    public required JsonElement Content { get; init; }
}
