using System.Collections.Concurrent;
using Moongate.UO.Data.Interfaces.Templates;
using Moongate.UO.Data.Templates.Mobiles;

namespace Moongate.UO.Data.Services.Templates;

/// <summary>
/// In-memory registry for mobile templates keyed by template id.
/// </summary>
public sealed class MobileTemplateService : IMobileTemplateService
{
    private readonly ConcurrentDictionary<string, MobileTemplateDefinition> _definitions =
        new(StringComparer.OrdinalIgnoreCase);

    public int Count => _definitions.Count;

    public void Clear()
        => _definitions.Clear();

    public IReadOnlyList<MobileTemplateDefinition> GetAll()
        => _definitions.Values.OrderBy(static definition => definition.Id, StringComparer.OrdinalIgnoreCase).ToList();

    public bool TryGet(string id, out MobileTemplateDefinition? definition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return _definitions.TryGetValue(id, out definition);
    }

    public void Upsert(MobileTemplateDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentException.ThrowIfNullOrWhiteSpace(definition.Id);

        _definitions[definition.Id] = definition;
    }

    public void UpsertRange(IEnumerable<MobileTemplateDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        foreach (var definition in definitions)
        {
            Upsert(definition);
        }
    }
}
