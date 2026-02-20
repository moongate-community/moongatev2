using System.Text.Json;
using Moongate.UO.Data.Templates.Startup;

namespace Moongate.UO.Data.Interfaces.Templates;

/// <summary>
/// Stores and resolves startup templates loaded from <c>templates/startup</c>.
/// </summary>
public interface IStartupTemplateService
{
    /// <summary>
    /// Gets the number of startup template documents currently registered.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Removes all registered startup templates.
    /// </summary>
    void Clear();

    /// <summary>
    /// Returns all startup templates as a snapshot.
    /// </summary>
    /// <returns>List of currently registered startup templates.</returns>
    IReadOnlyList<StartupTemplateDocument> GetAll();

    /// <summary>
    /// Tries to get a startup template by identifier.
    /// </summary>
    /// <param name="id">Template identifier.</param>
    /// <param name="document">Resolved document when present.</param>
    /// <returns><c>true</c> when found; otherwise <c>false</c>.</returns>
    bool TryGet(string id, out StartupTemplateDocument? document);

    /// <summary>
    /// Adds or replaces a startup template payload by identifier.
    /// </summary>
    /// <param name="id">Template identifier.</param>
    /// <param name="content">Template JSON payload.</param>
    void Upsert(string id, JsonElement content);
}
