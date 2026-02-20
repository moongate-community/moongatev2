using System.Collections.Concurrent;
using Moongate.UO.Data.Interfaces.Names;
using Moongate.UO.Data.Templates.Mobiles;

namespace Moongate.UO.Data.Services.Names;

/// <summary>
/// Default in-memory implementation of <see cref="INameService" />.
/// </summary>
public sealed class NameService : INameService
{
    private readonly ConcurrentDictionary<string, List<string>> _names = new(StringComparer.OrdinalIgnoreCase);
    private readonly Random _random = Random.Shared;

    /// <inheritdoc />
    public void AddNames(string type, params string[] names)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentNullException.ThrowIfNull(names);

        if (names.Length == 0)
        {
            return;
        }

        var pool = _names.GetOrAdd(type, static _ => []);

        lock (pool)
        {
            foreach (var name in names)
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    pool.Add(name);
                }
            }
        }
    }

    /// <inheritdoc />
    public string GenerateName(string type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);

        if (!_names.TryGetValue(type, out var pool))
        {
            return string.Empty;
        }

        lock (pool)
        {
            return pool.Count == 0 ? string.Empty : pool[_random.Next(pool.Count)];
        }
    }

    /// <inheritdoc />
    public string GenerateName(MobileTemplateDefinition mobileTemplate)
    {
        ArgumentNullException.ThrowIfNull(mobileTemplate);

        var fromId = GenerateName(mobileTemplate.Id);

        if (!string.IsNullOrEmpty(fromId))
        {
            return fromId;
        }

        var fromCategory = GenerateName(mobileTemplate.Category);

        if (!string.IsNullOrEmpty(fromCategory))
        {
            return fromCategory;
        }

        return GenerateName(mobileTemplate.Name);
    }
}
