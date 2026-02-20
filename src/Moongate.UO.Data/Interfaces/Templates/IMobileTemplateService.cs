using Moongate.UO.Data.Templates.Mobiles;

namespace Moongate.UO.Data.Interfaces.Templates;

/// <summary>
/// Stores and resolves mobile template definitions loaded from template files.
/// </summary>
public interface IMobileTemplateService
{
    /// <summary>
    /// Gets the number of registered mobile templates.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Removes all registered templates.
    /// </summary>
    void Clear();

    /// <summary>
    /// Adds or replaces a mobile template by identifier.
    /// </summary>
    /// <param name="definition">Template to register.</param>
    void Upsert(MobileTemplateDefinition definition);

    /// <summary>
    /// Adds or replaces multiple mobile templates.
    /// </summary>
    /// <param name="definitions">Templates to register.</param>
    void UpsertRange(IEnumerable<MobileTemplateDefinition> definitions);

    /// <summary>
    /// Tries to resolve a template by id.
    /// </summary>
    /// <param name="id">Template id.</param>
    /// <param name="definition">Resolved template when present.</param>
    /// <returns><c>true</c> when found; otherwise <c>false</c>.</returns>
    bool TryGet(string id, out MobileTemplateDefinition? definition);

    /// <summary>
    /// Gets all templates as a snapshot list.
    /// </summary>
    /// <returns>All registered templates.</returns>
    IReadOnlyList<MobileTemplateDefinition> GetAll();
}
