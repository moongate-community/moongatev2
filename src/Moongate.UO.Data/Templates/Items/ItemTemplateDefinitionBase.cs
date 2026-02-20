using System.Text.Json.Serialization;

namespace Moongate.UO.Data.Templates.Items;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type"), JsonDerivedType(typeof(ItemTemplateDefinition), "item")]
public abstract class ItemTemplateDefinitionBase
{
    public string Category { get; set; }

    public string Id { get; set; }

    public string Name { get; set; }
}
