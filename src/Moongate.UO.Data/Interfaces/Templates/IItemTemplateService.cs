using Moongate.UO.Data.Templates.Items;

namespace Moongate.UO.Data.Interfaces.Templates;

/// <summary>
/// Stores and resolves item template definitions loaded from template files.
/// </summary>
public interface IItemTemplateService
{
    /// <summary>
    /// Gets the number of templates currently registered.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Removes all registered templates.
    /// </summary>
    void Clear();

    /// <summary>
    /// Returns all templates as a snapshot.
    /// </summary>
    /// <returns>List of currently registered templates.</returns>
    IReadOnlyList<ItemTemplateDefinition> GetAll();

    /// <summary>
    /// Tries to get a template by id.
    /// </summary>
    /// <param name="id">Template id.</param>
    /// <param name="definition">Resolved template when present.</param>
    /// <returns><c>true</c> when found; otherwise <c>false</c>.</returns>
    bool TryGet(string id, out ItemTemplateDefinition? definition);

    /// <summary>
    /// Adds or replaces a template by identifier.
    /// </summary>
    /// <param name="definition">Template instance to register.</param>
    void Upsert(ItemTemplateDefinition definition);

    /// <summary>
    /// Adds or replaces multiple templates.
    /// </summary>
    /// <param name="templates">Templates to register.</param>
    void UpsertRange(IEnumerable<ItemTemplateDefinition> templates);
}
