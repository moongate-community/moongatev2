using System.Text.Json.Serialization;

namespace Moongate.UO.Data.Templates.Mobiles;

/// <summary>
/// Base DTO for polymorphic mobile templates.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type"), JsonDerivedType(typeof(MobileTemplateDefinition), "mobile")]
public abstract class MobileTemplateDefinitionBase
{
    public string Category { get; set; }

    public string Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public List<string> Tags { get; set; } = [];
}
