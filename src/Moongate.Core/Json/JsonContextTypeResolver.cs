using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Moongate.Core.Json;

/// <summary>
/// Utility class to retrieve registered types from JsonSerializerContext at runtime.
/// </summary>
public static class JsonContextTypeResolver
{
    /// <summary>
    /// Gets all types registered in the specified JsonSerializerContext.
    /// </summary>
    /// <param name="context">The JsonSerializerContext to query.</param>
    /// <returns>A collection of registered Type objects.</returns>
    public static IEnumerable<Type> GetRegisteredTypes(JsonSerializerContext context)
    {
        // Get all JsonTypeInfo properties from the context
        var properties = GetContextType(context)
                         .GetProperties()
                         .Where(
                             p => p.PropertyType.IsGenericType &&
                                  p.PropertyType.GetGenericTypeDefinition() == typeof(JsonTypeInfo<>)
                         );

        foreach (var property in properties)
        {
            // Extract the generic type argument (the actual registered type)
            var typeInfo = property.PropertyType.GetGenericArguments()[0];

            yield return typeInfo;
        }
    }

    /// <summary>
    /// Gets all registered types that inherit from a specific base type.
    /// </summary>
    /// <param name="context">The JsonSerializerContext to query.</param>
    /// <typeparam name="TBase">The base type to filter by.</typeparam>
    /// <returns>A collection of types that inherit from TBase.</returns>
    public static IEnumerable<Type> GetRegisteredTypes<TBase>(JsonSerializerContext context)
    {
        return GetRegisteredTypes(context)
            .Where(t => typeof(TBase).IsAssignableFrom(t));
    }

    /// <summary>
    /// Gets all registered types with their corresponding JsonTypeInfo.
    /// </summary>
    /// <param name="context">The JsonSerializerContext to query.</param>
    /// <returns>A dictionary mapping Type to JsonTypeInfo.</returns>
    public static Dictionary<Type, JsonTypeInfo> GetRegisteredTypesWithInfo(JsonSerializerContext context)
    {
        var result = new Dictionary<Type, JsonTypeInfo>();

        var properties = GetContextType(context)
                         .GetProperties()
                         .Where(
                             p => p.PropertyType.IsGenericType &&
                                  p.PropertyType.GetGenericTypeDefinition() == typeof(JsonTypeInfo<>)
                         );

        foreach (var property in properties)
        {
            var typeInfo = property.PropertyType.GetGenericArguments()[0];
            var jsonTypeInfo = property.GetValue(context) as JsonTypeInfo;

            if (jsonTypeInfo != null)
            {
                result[typeInfo] = jsonTypeInfo;
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the JsonTypeInfo for a specific type if registered.
    /// </summary>
    /// <param name="context">The JsonSerializerContext to query.</param>
    /// <typeparam name="T">The type to get info for.</typeparam>
    /// <returns>The JsonTypeInfo if found, null otherwise.</returns>
    public static JsonTypeInfo<T>? GetTypeInfo<T>(JsonSerializerContext context)
    {
        var propertyName = typeof(T).Name;

        var property = GetContextType(context)
            .GetProperty(propertyName, typeof(JsonTypeInfo<T>));

        return property?.GetValue(context) as JsonTypeInfo<T>;
    }

    /// <summary>
    /// Checks if a specific type is registered in the context.
    /// </summary>
    /// <param name="context">The JsonSerializerContext to query.</param>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is registered, false otherwise.</returns>
    public static bool IsTypeRegistered(JsonSerializerContext context, Type type)
        => GetRegisteredTypes(context).Contains(type);

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2073",
        Justification = "JsonSerializerContext types are preserved by design"
    )]
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    private static Type GetContextType(JsonSerializerContext context)
        => context.GetType();
}
