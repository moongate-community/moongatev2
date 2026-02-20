using System.Collections.Concurrent;
using System.Text.Json;
using Moongate.UO.Data.Interfaces.Templates;
using Moongate.UO.Data.Templates.Startup;

namespace Moongate.UO.Data.Services.Templates;

/// <summary>
/// Thread-safe startup template store keyed by template identifier.
/// </summary>
public sealed class StartupTemplateService : IStartupTemplateService
{
    private readonly ConcurrentDictionary<string, StartupTemplateDocument> _documents =
        new(StringComparer.OrdinalIgnoreCase);

    public int Count => _documents.Count;

    /// <inheritdoc />
    public void Clear()
        => _documents.Clear();

    /// <inheritdoc />
    public IReadOnlyList<StartupTemplateDocument> GetAll()
        => _documents.Values.OrderBy(static document => document.Id, StringComparer.OrdinalIgnoreCase).ToList();

    /// <inheritdoc />
    public bool TryGet(string id, out StartupTemplateDocument? document)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return _documents.TryGetValue(id, out document);
    }

    /// <inheritdoc />
    public void Upsert(string id, JsonElement content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        _documents[id] = new()
        {
            Id = id,
            Content = content.Clone()
        };
    }
}
