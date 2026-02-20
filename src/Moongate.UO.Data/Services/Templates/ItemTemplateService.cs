using System.Collections.Concurrent;
using Moongate.UO.Data.Interfaces.Templates;
using Moongate.UO.Data.Templates.Items;

namespace Moongate.UO.Data.Services.Templates;

public sealed class ItemTemplateService : IItemTemplateService
{
    private readonly ConcurrentDictionary<string, ItemTemplateDefinition> _templates = new(StringComparer.OrdinalIgnoreCase);

    public int Count => _templates.Count;

    public void Clear()
        => _templates.Clear();

    public void Upsert(ItemTemplateDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentException.ThrowIfNullOrWhiteSpace(definition.Id);

        _templates[definition.Id] = definition;
    }

    public void UpsertRange(IEnumerable<ItemTemplateDefinition> templates)
    {
        ArgumentNullException.ThrowIfNull(templates);

        foreach (var definition in templates)
        {
            Upsert(definition);
        }
    }

    public bool TryGet(string id, out ItemTemplateDefinition? definition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return _templates.TryGetValue(id, out definition);
    }

    public IReadOnlyList<ItemTemplateDefinition> GetAll()
        => _templates.Values.OrderBy(static template => template.Id, StringComparer.OrdinalIgnoreCase).ToList();
}
