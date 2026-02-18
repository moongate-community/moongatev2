using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Moongate.Core.Json;

/// <summary>
/// Provides utility methods for JSON serialization and deserialization.
/// </summary>
public static class JsonUtils
{
    private static readonly ConcurrentBag<IJsonTypeInfoResolver> JsonSerializerContexts = new();
    private static readonly ConcurrentBag<JsonConverter> JsonConverters = new();
    private static readonly Lock _lockObject = new();

    private static volatile JsonSerializerOptions? _jsonSerializerOptions;

    [UnconditionalSuppressMessage(
        "Aot",
        "IL3050",
        Justification = "Global enum string conversion is intentionally configured via JsonStringEnumConverter."
    )]
    static JsonUtils()
    {
        // Add default converters
        JsonConverters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        RebuildJsonSerializerContexts();
    }

    /// <summary>
    /// Adds a JSON converter to the global converter list. Thread-safe.
    /// </summary>
    /// <param name="converter">The converter to add.</param>
    public static void AddJsonConverter(JsonConverter converter)
    {
        ArgumentNullException.ThrowIfNull(converter);

        // Check if converter type already exists
        var converterType = converter.GetType();

        if (JsonConverters.Any(c => c.GetType() == converterType))
        {
            return; // Prevent duplicates
        }

        JsonConverters.Add(converter);
        RebuildJsonSerializerContexts();
    }

    /// <summary>
    /// Deserializes a JSON string to an object using global options.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>Deserialized object.</returns>
    [RequiresUnreferencedCode(
         "JSON deserialization may require types that cannot be statically analyzed. Use overload with JsonSerializerContext when possible."
     ),
     RequiresDynamicCode(
         "JSON deserialization may require dynamic code generation. Use overload with JsonSerializerContext when possible."
     )]

    /// <summary>
    /// Deserializes a JSON string to an object using global options.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>Deserialized object.</returns>
    public static T? Deserialize<T>(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        try
        {
            return JsonSerializer.Deserialize<T>(json, GetJsonSerializerOptions()) ??
                   throw new JsonException($"Deserialization returned null for type {typeof(T).Name}");
        }
        catch (JsonException ex)
        {
            throw new JsonException($"Failed to deserialize JSON to type {typeof(T).Name}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deserializes an object from a JSON file.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="filePath">Path to the JSON file.</param>
    /// <returns>Deserialized object.</returns>
    [RequiresUnreferencedCode(
         "JSON deserialization may require types that cannot be statically analyzed. Use overload with JsonSerializerContext when possible."
     ),
     RequiresDynamicCode(
         "JSON deserialization may require dynamic code generation. Use overload with JsonSerializerContext when possible."
     )]

    /// <summary>
    /// Deserializes an object from a JSON file.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="filePath">Path to the JSON file.</param>
    /// <returns>Deserialized object.</returns>
    public static T DeserializeFromFile<T>(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var normalizedPath = Path.GetFullPath(filePath);

        if (!File.Exists(normalizedPath))
        {
            throw new FileNotFoundException($"The file '{normalizedPath}' does not exist.", normalizedPath);
        }

        try
        {
            var json = File.ReadAllText(normalizedPath);

            return Deserialize<T>(json);
        }
        catch (Exception ex) when (ex is not JsonException)
        {
            throw new JsonException($"Failed to read or deserialize file '{normalizedPath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deserializes an object from a JSON file using a JsonSerializerContext.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="filePath">Path to the JSON file.</param>
    /// <param name="context">The JsonSerializerContext to use for deserialization.</param>
    /// <returns>Deserialized object.</returns>
    public static T DeserializeFromFile<T>(string filePath, JsonSerializerContext context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(context);

        var normalizedPath = Path.GetFullPath(filePath);

        if (!File.Exists(normalizedPath))
        {
            throw new FileNotFoundException($"The file '{normalizedPath}' does not exist.", normalizedPath);
        }

        try
        {
            var json = File.ReadAllText(normalizedPath);
            var result = JsonSerializer.Deserialize(json, context.GetTypeInfo(typeof(T)));

            return result is T typedResult
                       ? typedResult
                       : throw new JsonException($"Deserialization returned null for type {typeof(T).Name}");
        }
        catch (JsonException ex)
        {
            throw new JsonException(
                $"Failed to deserialize file '{normalizedPath}' to type {typeof(T).Name}: {ex.Message}",
                ex
            );
        }
        catch (Exception ex) when (ex is not JsonException)
        {
            throw new JsonException($"Failed to read or deserialize file '{normalizedPath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deserializes an object from a JSON file asynchronously.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="filePath">Path to the JSON file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deserialized object.</returns>
    [RequiresUnreferencedCode(
         "JSON deserialization may require types that cannot be statically analyzed. Use overload with JsonSerializerContext when possible."
     ),
     RequiresDynamicCode(
         "JSON deserialization may require dynamic code generation. Use overload with JsonSerializerContext when possible."
     )]

    /// <summary>
    /// Deserializes an object from a JSON file asynchronously.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="filePath">Path to the JSON file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deserialized object.</returns>
    public static async Task<T> DeserializeFromFileAsync<T>(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var normalizedPath = Path.GetFullPath(filePath);

        if (!File.Exists(normalizedPath))
        {
            throw new FileNotFoundException($"The file '{normalizedPath}' does not exist.", normalizedPath);
        }

        try
        {
            var json = await File.ReadAllTextAsync(normalizedPath, cancellationToken).ConfigureAwait(false);

            return Deserialize<T>(json);
        }
        catch (Exception ex) when (ex is not JsonException)
        {
            throw new JsonException($"Failed to read or deserialize file '{normalizedPath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deserializes an object from a stream asynchronously.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="stream">The stream containing JSON data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deserialized object.</returns>
    [RequiresUnreferencedCode(
         "JSON deserialization may require types that cannot be statically analyzed. Use overload with JsonSerializerContext when possible."
     ),
     RequiresDynamicCode(
         "JSON deserialization may require dynamic code generation. Use overload with JsonSerializerContext when possible."
     )]

    /// <summary>
    /// Deserializes an object from a stream asynchronously.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="stream">The stream containing JSON data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deserialized object.</returns>
    public static async Task<T> DeserializeFromStreamAsync<T>(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        try
        {
            var result = await JsonSerializer.DeserializeAsync<T>(stream, GetJsonSerializerOptions(), cancellationToken)
                                             .ConfigureAwait(false);

            return result ?? throw new JsonException($"Deserialization returned null for type {typeof(T).Name}");
        }
        catch (JsonException ex)
        {
            throw new JsonException($"Failed to deserialize stream to type {typeof(T).Name}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deserializes a JSON string to an object with fallback value.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="defaultValue">Default value if deserialization fails.</param>
    /// <returns>Deserialized object or default value.</returns>
    [RequiresUnreferencedCode(
         "JSON deserialization may require types that cannot be statically analyzed. Use overload with JsonSerializerContext when possible."
     ),
     RequiresDynamicCode(
         "JSON deserialization may require dynamic code generation. Use overload with JsonSerializerContext when possible."
     )]

    /// <summary>
    /// Deserializes a JSON string to an object with fallback value.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="defaultValue">Default value if deserialization fails.</param>
    /// <returns>Deserialized object or default value.</returns>
    public static T? DeserializeOrDefault<T>(string json, T? defaultValue = default)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return defaultValue;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, GetJsonSerializerOptions()) ?? defaultValue;
        }
        catch (JsonException)
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Gets a read-only view of the current JSON converters.
    /// </summary>
    /// <returns>A read-only list of JSON converters.</returns>
    public static IReadOnlyList<JsonConverter> GetJsonConverters()
    {
        // Avoid ToList() allocation - JsonConverters is already thread-safe
        var converters = new JsonConverter[JsonConverters.Count];
        JsonConverters.CopyTo(converters, 0);

        return Array.AsReadOnly(converters);
    }

    /// <summary>
    /// Gets the current JSON serializer options. Thread-safe.
    /// </summary>
    public static JsonSerializerOptions GetJsonSerializerOptions()
        => _jsonSerializerOptions ?? throw new InvalidOperationException("JsonSerializerOptions not initialized");

    /// <summary>
    /// Generates a schema file name for the given type using snake_case convention.
    /// </summary>
    /// <param name="type">The type to generate a schema name for.</param>
    /// <returns>A schema file name in the format "type_name.schema.json".</returns>
    public static string GetSchemaFileName(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var typeName = type.Name;

        // Remove "Entity" suffix if present
        if (typeName.EndsWith("Entity", StringComparison.Ordinal))
        {
            typeName = typeName[..^6]; // Remove last 6 characters ("Entity")
        }

        // Convert PascalCase to snake_case
        var snakeCaseName = ConvertToSnakeCase(typeName);

        return $"{snakeCaseName}.schema.json";
    }

    /// <summary>
    /// Determines if a JSON string represents an array.
    /// </summary>
    /// <param name="json">The JSON string to analyze.</param>
    /// <returns>True if the JSON represents an array, false otherwise.</returns>
    public static bool IsArray(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(json);

            return document.RootElement.ValueKind == JsonValueKind.Array;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Determines if a JSON file contains an array.
    /// </summary>
    /// <param name="filePath">Path to the JSON file to analyze.</param>
    /// <returns>True if the JSON file contains an array, false otherwise.</returns>
    public static bool IsArrayFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var normalizedPath = Path.GetFullPath(filePath);

        if (!File.Exists(normalizedPath))
        {
            return false;
        }

        try
        {
            var json = File.ReadAllText(normalizedPath);

            return IsArray(json);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Validates if a string is valid JSON without deserializing.
    /// </summary>
    /// <param name="json">The JSON string to validate.</param>
    /// <returns>True if valid JSON, false otherwise.</returns>
    public static bool IsValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(json);

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Registers a JSON serializer context for source generation. Thread-safe.
    /// </summary>
    /// <param name="context">The context to register.</param>
    public static void RegisterJsonContext(JsonSerializerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        JsonSerializerContexts.Add(context);
        RebuildJsonSerializerContexts();
    }

    /// <summary>
    /// Removes all converters of the specified type. Thread-safe.
    /// </summary>
    /// <typeparam name="T">The converter type to remove.</typeparam>
    /// <returns>True if any converters were removed.</returns>
    public static bool RemoveJsonConverter<T>() where T : JsonConverter
    {
        var removed = false;
        var newConverters = new ConcurrentBag<JsonConverter>();

        foreach (var converter in JsonConverters)
        {
            if (converter is not T)
            {
                newConverters.Add(converter);
            }
            else
            {
                removed = true;
            }
        }

        if (removed)
        {
            // Replace the collection - this is not atomic, but thread-safe enough for this use case
            JsonConverters.Clear();

            foreach (var converter in newConverters)
            {
                JsonConverters.Add(converter);
            }

            RebuildJsonSerializerContexts();
        }

        return removed;
    }

    /// <summary>
    /// Serializes an object to JSON string using global options.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>JSON string representation.</returns>
    [RequiresUnreferencedCode(
         "JSON serialization may require types that cannot be statically analyzed. Use overload with JsonSerializerContext when possible."
     ),
     RequiresDynamicCode(
         "JSON serialization may require dynamic code generation. Use overload with JsonSerializerContext when possible."
     )]

    /// <summary>
    /// Serializes an object to JSON string using global options.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>JSON string representation.</returns>
    public static string Serialize<T>(T obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        try
        {
            return JsonSerializer.Serialize(obj, GetJsonSerializerOptions());
        }
        catch (JsonException ex)
        {
            throw new JsonException($"Failed to serialize object of type {typeof(T).Name}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Serializes an object to JSON string using custom options.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="options">Custom serialization options.</param>
    /// <returns>JSON string representation.</returns>
    [RequiresUnreferencedCode(
         "JSON serialization may require types that cannot be statically analyzed. Use overload with JsonSerializerContext when possible."
     ),
     RequiresDynamicCode(
         "JSON serialization may require dynamic code generation. Use overload with JsonSerializerContext when possible."
     )]

    /// <summary>
    /// Serializes an object to JSON string using custom options.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="options">Custom serialization options.</param>
    /// <returns>JSON string representation.</returns>
    public static string Serialize<T>(T obj, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(options);

        try
        {
            return JsonSerializer.Serialize(obj, options);
        }
        catch (JsonException ex)
        {
            throw new JsonException($"Failed to serialize object of type {typeof(T).Name}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Serializes multiple objects to JSON files in a directory.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="objects">Dictionary of filename to object mappings.</param>
    /// <param name="directory">Target directory path.</param>
    [RequiresUnreferencedCode(
         "JSON serialization may require types that cannot be statically analyzed. Use overload with JsonSerializerContext when possible."
     ),
     RequiresDynamicCode(
         "JSON serialization may require dynamic code generation. Use overload with JsonSerializerContext when possible."
     )]

    /// <summary>
    /// Serializes multiple objects to JSON files in a directory.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="objects">Dictionary of filename to object mappings.</param>
    /// <param name="directory">Target directory path.</param>
    public static void SerializeMultipleToDirectory<T>(Dictionary<string, T> objects, string directory)
    {
        ArgumentNullException.ThrowIfNull(objects);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);

        var normalizedDirectory = Path.GetFullPath(directory);

        if (!Directory.Exists(normalizedDirectory))
        {
            Directory.CreateDirectory(normalizedDirectory);
        }

        foreach (var kvp in objects)
        {
            if (kvp.Value == null)
            {
                continue;
            }

            var fileName = Path.GetFileNameWithoutExtension(kvp.Key);
            var filePath = Path.Combine(normalizedDirectory, $"{fileName}.json");
            SerializeToFile(kvp.Value, filePath);
        }
    }

    /// <summary>
    /// Serializes an object to a JSON file with directory creation.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="filePath">Path to the output JSON file.</param>
    [RequiresUnreferencedCode(
         "JSON serialization may require types that cannot be statically analyzed. Use overload with JsonSerializerContext when possible."
     ),
     RequiresDynamicCode(
         "JSON serialization may require dynamic code generation. Use overload with JsonSerializerContext when possible."
     )]

    /// <summary>
    /// Serializes an object to a JSON file with directory creation.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="filePath">Path to the output JSON file.</param>
    public static void SerializeToFile<T>(T obj, string filePath)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var normalizedPath = Path.GetFullPath(filePath);
        var directory = Path.GetDirectoryName(normalizedPath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        try
        {
            var json = Serialize(obj);
            File.WriteAllText(normalizedPath, json);
        }
        catch (Exception ex) when (ex is not JsonException)
        {
            throw new JsonException($"Failed to serialize or write file '{normalizedPath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Serializes an object to a JSON file with directory creation using a JsonSerializerContext.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="filePath">Path to the output JSON file.</param>
    /// <param name="context">The JsonSerializerContext to use for serialization.</param>
    public static void SerializeToFile<T>(T obj, string filePath, JsonSerializerContext context)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(context);

        var normalizedPath = Path.GetFullPath(filePath);
        var directory = Path.GetDirectoryName(normalizedPath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        try
        {
            var typeInfo = context.GetTypeInfo(typeof(T));
            var json = JsonSerializer.Serialize(obj, typeInfo);
            File.WriteAllText(normalizedPath, json);
        }
        catch (Exception ex) when (ex is not JsonException)
        {
            throw new JsonException($"Failed to serialize or write file '{normalizedPath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Serializes an object to a JSON file asynchronously with directory creation.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="filePath">Path to the output JSON file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [RequiresUnreferencedCode(
         "JSON serialization may require types that cannot be statically analyzed. Use overload with JsonSerializerContext when possible."
     ),
     RequiresDynamicCode(
         "JSON serialization may require dynamic code generation. Use overload with JsonSerializerContext when possible."
     )]

    /// <summary>
    /// Serializes an object to a JSON file asynchronously with directory creation.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="filePath">Path to the output JSON file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task SerializeToFileAsync<T>(T obj, string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var normalizedPath = Path.GetFullPath(filePath);
        var directory = Path.GetDirectoryName(normalizedPath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        try
        {
            var json = Serialize(obj);
            await File.WriteAllTextAsync(normalizedPath, json, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not JsonException)
        {
            throw new JsonException($"Failed to serialize or write file '{normalizedPath}': {ex.Message}", ex);
        }
    }

    private static string ConvertToSnakeCase(string pascalCase)
    {
        if (string.IsNullOrEmpty(pascalCase))
        {
            return pascalCase;
        }

        var result = new StringBuilder();
        result.Append(char.ToLowerInvariant(pascalCase[0]));

        for (var i = 1; i < pascalCase.Length; i++)
        {
            if (char.IsUpper(pascalCase[i]))
            {
                result.Append('_');
                result.Append(char.ToLowerInvariant(pascalCase[i]));
            }
            else
            {
                result.Append(pascalCase[i]);
            }
        }

        return result.ToString();
    }

    private static void RebuildJsonSerializerContexts()
    {
        lock (_lockObject)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                TypeInfoResolver = JsonTypeInfoResolver.Combine(JsonSerializerContexts.ToArray())
            };

            foreach (var converter in JsonConverters)
            {
                options.Converters.Add(converter);
            }

            _jsonSerializerOptions = options;
        }
    }
}
