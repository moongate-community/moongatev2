using Moongate.Core.Random;

namespace Moongate.Core.Extensions.Collections;

public static class CollectionExtensions
{
    public static T RandomElement<T>(this IReadOnlyCollection<T> collection)
    {
        if (collection == null || collection.Count == 0)
        {
            throw new ArgumentException("Collection cannot be null or empty.", nameof(collection));
        }

        var randomIndex = BuiltInRng.Next(0, collection.Count);

        return collection.ElementAt(randomIndex);
    }
}
