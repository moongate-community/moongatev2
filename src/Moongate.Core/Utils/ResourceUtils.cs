using System.Reflection;
using System.Text.RegularExpressions;

namespace DarkLilly.Core.Utils;

/// <summary>
/// Provides utilities for working with embedded resources.
/// </summary>
public static partial class ResourceUtils
{
    /// <summary>
    /// Converts a resource name to a file path format.
    /// </summary>
    /// <param name="resourceName">The resource name to convert.</param>
    /// <param name="baseNamespace">The base namespace to remove.</param>
    /// <returns>The file path.</returns>
    /// <summary>
    /// </summary>
    public static string ConvertResourceNameToPath(string resourceName, string baseNamespace)
    {
        if (!resourceName.StartsWith(baseNamespace + ".", StringComparison.Ordinal))
        {
            throw new ArgumentException("Resource name does not start with the given base namespace.");
        }

        var relativeName = resourceName[(baseNamespace.Length + 1)..];

        var lastDotIndex = relativeName.LastIndexOf('.');

        if (lastDotIndex == -1)
        {
            throw new ArgumentException("Resource name does not contain a valid extension.");
        }

        var pathPart = relativeName[..lastDotIndex].Replace('.', Path.DirectorySeparatorChar);
        var extension = relativeName[(lastDotIndex + 1)..];

        return $"{pathPart}.{extension}";
    }

    /// <summary>
    /// Copies all embedded resources from an assembly to a destination directory
    /// </summary>
    /// <param name="assembly">The assembly containing the embedded resources</param>
    /// <param name="destinationDirectory">The directory where resources will be copied</param>
    public static void CopyEmbeddedToDirectory(Assembly assembly, string destinationDirectory)
    {
        if (!Directory.Exists(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        var resourceNames = assembly.GetManifestResourceNames();
        var assemblyName = assembly.GetName().Name;

        foreach (var resourceName in resourceNames)
        {
            var relativePath = ConvertResourceNameToPath(resourceName, assemblyName);
            var destinationPath = Path.Combine(destinationDirectory, relativePath);

            // Create directory if it doesn't exist
            var directory = Path.GetDirectoryName(destinationPath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Copy the resource content to the file
            var resourceContent = GetEmbeddedResourceContent(resourceName[(assemblyName!.Length + 1)..], assembly);
            File.WriteAllBytes(destinationPath, resourceContent);
        }
    }

    /// <summary>
    /// Converts an embedded resource name to a file path format
    /// </summary>
    /// <param name="resourceName">The full embedded resource name</param>
    /// <param name="assemblyPrefix">The assembly prefix to remove from the resource name</param>
    /// <returns>A file path representation of the resource name</returns>
    public static string EmbeddedNameToPath(string resourceName, string assemblyPrefix)
    {
        if (resourceName.StartsWith(assemblyPrefix + ".", StringComparison.Ordinal))
        {
            resourceName = resourceName[(assemblyPrefix.Length + 1)..];
        }

        return resourceName.Replace('.', '/');
    }

    /// <summary>
    /// Converts an embedded resource name to a directory path structure
    /// </summary>
    /// <param name="resourceName">The embedded resource name (e.g., "Assets.Fonts.DefaultUiFont.ttf")</param>
    /// <param name="baseNamespace">Optional base namespace to remove from the beginning</param>
    /// <returns>The directory path (e.g., "Assets/Fonts")</returns>
    public static string GetDirectoryPathFromResourceName(string resourceName, string? baseNamespace = null)
    {
        ArgumentNullException.ThrowIfNull(resourceName);

        // Remove base namespace if provided
        var workingName = resourceName;

        if (!string.IsNullOrEmpty(baseNamespace) && resourceName.StartsWith(baseNamespace + ".", StringComparison.Ordinal))
        {
            workingName = resourceName[(baseNamespace.Length + 1)..];
        }

        // Find the last dot that separates the file extension
        var lastDotIndex = workingName.LastIndexOf('.');

        if (lastDotIndex == -1)
        {
            return string.Empty; // No extension, no directory
        }

        // Find the second-to-last dot that separates the filename from the path
        var secondLastDotIndex = workingName.LastIndexOf('.', lastDotIndex - 1);

        if (secondLastDotIndex == -1)
        {
            return string.Empty; // Only one dot, no directory
        }

        // Extract directory path and convert dots to path separators
        var directoryPart = workingName[..secondLastDotIndex];

        return directoryPart.Replace('.', Path.DirectorySeparatorChar);
    }

    /// <summary>
    /// Gets an embedded resource as a byte array wrapped in Memory
    /// </summary>
    /// <param name="assembly">The assembly containing the resource</param>
    /// <param name="resourceName">The full resource name</param>
    /// <returns>A Memory containing the resource bytes</returns>
    /// <exception cref="FileNotFoundException">Thrown when the resource cannot be found</exception>
    public static Memory<byte> GetEmbeddedResourceByteArray(Assembly assembly, string resourceName)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(resourceName);

        var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream == null)
        {
            // Try to find a partial match
            var resourceNames = assembly.GetManifestResourceNames();
            var matchingResource = resourceNames.FirstOrDefault(
                n => n.EndsWith(
                    resourceName.Replace('/', '.').Replace('\\', '.'),
                    StringComparison.Ordinal
                )
            );

            if (matchingResource != null)
            {
                stream = assembly.GetManifestResourceStream(matchingResource);
            }
        }

        if (stream == null)
        {
            throw new FileNotFoundException($"Embedded resource not found: {resourceName}");
        }

        using (stream)
        {
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            return new(memoryStream.ToArray());
        }
    }

    /// <summary>
    /// Reads the content of an embedded resource as a byte array
    /// </summary>
    /// <param name="resourcePath">Resource path (e.g. "Assets/Templates/welcome.scriban")</param>
    /// <param name="assembly">The assembly to search in (if null, uses current assembly)</param>
    /// <returns>The content of the resource as a byte array</returns>
    public static byte[] GetEmbeddedResourceContent(string resourcePath, Assembly assembly = null)
    {
        assembly ??= Assembly.GetExecutingAssembly();

        // Normalize the path for embedded resource format
        var normalizedPath = resourcePath.Replace('/', '.').Replace('\\', '.');

        // Get the full resource name
        var assemblyName = assembly.GetName().Name;
        var fullResourceName = $"{assemblyName}.{normalizedPath}";

        // Check if the resource exists
        if (!assembly.GetManifestResourceNames().Contains(fullResourceName))
        {
            // Try to find a partial match
            var resourceNames = assembly.GetManifestResourceNames();
            var matchingResource = resourceNames.FirstOrDefault(n => n.EndsWith(normalizedPath, StringComparison.Ordinal));

            if (matchingResource != null)
            {
                fullResourceName = matchingResource;
            }
            else
            {
                throw new FileNotFoundException($"Embedded resource not found: {resourcePath}");
            }
        }

        // Read the resource content
        using var stream = assembly.GetManifestResourceStream(fullResourceName);

        if (stream == null)
        {
            throw new FileNotFoundException($"Unable to open resource: {fullResourceName}");
        }

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);

        return memoryStream.ToArray();
    }

    /// <summary>
    /// Gets a list of all files in a specific embedded directory
    /// </summary>
    /// <param name="assembly">The assembly to search in (if null, uses current assembly)</param>
    /// <param name="directoryPath">Directory path to search (e.g. "Assets/Templates")</param>
    /// <returns>A list of file names (without the full path)</returns>
    public static List<string> GetEmbeddedResourceFileNames(
        Assembly assembly = null,
        string directoryPath = "Assets/Templates"
    )
    {
        // Normalize the path for embedded resource format
        var normalizedPath = directoryPath.Replace('/', '.').Replace('\\', '.');

        // Get all resources in the specified path
        var resources = GetEmbeddedResourceNames(assembly, normalizedPath);

        // Extract file names from the full paths
        var fileNames = new List<string>();

        foreach (var resource in resources)
        {
            // Extract the final part of the resource name (file name with extension)
            var fileName = resource.Substring(resource.LastIndexOf('.') + 1);

            // If not empty, add it to the list
            if (!string.IsNullOrEmpty(fileName))
            {
                fileNames.Add(fileName);
            }
        }

        return fileNames;
    }

    /// <summary>
    /// Gets a list of all embedded resources that match a given pattern
    /// </summary>
    /// <param name="assembly">The assembly to search in (if null, uses current assembly)</param>
    /// <param name="directoryPath">Directory path to search (e.g. "Assets.Templates")</param>
    /// <returns>A list of resource names found</returns>
    public static string[] GetEmbeddedResourceNames(Assembly assembly = null, string directoryPath = null)
    {
        // If no assembly is specified, use the current one
        assembly ??= Assembly.GetExecutingAssembly();

        // Get all resources in the assembly
        var resourceNames = assembly.GetManifestResourceNames();

        // If no directory path is specified, return all resources
        if (string.IsNullOrEmpty(directoryPath))
        {
            return resourceNames;
        }

        // Replace any path separators with dots, as required by the embedded resource format
        var normalizedPath = directoryPath.Replace('/', '.').Replace('\\', '.');

        // If it doesn't end with a dot, add one to ensure we're looking for that specific path
        if (!normalizedPath.EndsWith('.'))
        {
            normalizedPath += ".";
        }

        // Filter resources that contain the specified path
        return resourceNames.Where(name => name.Contains(normalizedPath)).ToArray();
    }

    /// <summary>
    /// Gets a stream for an embedded resource
    /// </summary>
    /// <param name="assembly">The assembly containing the resource</param>
    /// <param name="resourceName">The full resource name</param>
    /// <returns>A stream for the resource</returns>
    /// <exception cref="FileNotFoundException">Thrown when the resource cannot be found</exception>
    public static Stream GetEmbeddedResourceStream(Assembly assembly, string resourceName)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(resourceName);

        var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream == null)
        {
            // Try to find a partial match
            var resourceNames = assembly.GetManifestResourceNames();
            var matchingResource = resourceNames.FirstOrDefault(
                n => n.EndsWith(
                    resourceName.Replace('/', '.').Replace('\\', '.'),
                    StringComparison.Ordinal
                )
            );

            if (matchingResource != null)
            {
                stream = assembly.GetManifestResourceStream(matchingResource);
            }
        }

        if (stream == null)
        {
            throw new FileNotFoundException($"Embedded resource not found: {resourceName}");
        }

        return stream;
    }

    public static string GetEmbeddedResourceString(Assembly assembly, string resourceName)
    {
        var stream = GetEmbeddedResourceStream(assembly, resourceName);

        using (stream)
        {
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }

    /// <summary>
    /// Converts an embedded resource name to a proper file name by extracting the last part after the final dot
    /// and treating everything before it as directory structure
    /// </summary>
    /// <param name="resourceName">The embedded resource name (e.g., "Assets.Fonts.DefaultUiFont.ttf")</param>
    /// <returns>The file name with extension (e.g., "DefaultUiFont.ttf")</returns>
    public static string GetFileNameFromResourceName(string resourceName)
    {
        ArgumentNullException.ThrowIfNull(resourceName);

        // Find the last dot that separates the file extension
        var lastDotIndex = resourceName.LastIndexOf('.');

        if (lastDotIndex == -1)
        {
            return resourceName; // No extension, return as is
        }

        // Find the second-to-last dot that separates the filename from the path
        var secondLastDotIndex = resourceName.LastIndexOf('.', lastDotIndex - 1);

        if (secondLastDotIndex == -1)
        {
            return resourceName; // Only one dot, return as is
        }

        // Extract filename + extension
        return resourceName[(secondLastDotIndex + 1)..];
    }

    /// <summary>
    /// Extracts the file name from an embedded resource path
    /// </summary>
    /// <param name="resourceName">Full resource name</param>
    /// <returns>File name without path</returns>
    public static string GetFileNameFromResourcePath(string resourceName)
    {
        // Use a regex to extract the file name
        var match = FileNameRegex().Match(resourceName);

        return
            match.Success ? match.Groups[1].Value : resourceName; // If it fails to find a pattern, return the original name
    }

    /// <summary>
    /// Reads the content of an embedded resource as a string.
    /// </summary>
    /// <param name="resourceName">The name of the resource to read.</param>
    /// <param name="assembly">The assembly containing the resource.</param>
    /// <returns>The content of the resource as a string.</returns>
    /// <exception cref="Exception">Thrown when the resource cannot be found in the specified assembly.</exception>
    /// <remarks>
    /// This method handles resource names that may contain either forward slashes (/) or
    /// backslashes (\) by converting them to dots, which is the standard separator for
    /// resource names in .NET assemblies.
    /// </remarks>
    public static string? ReadEmbeddedResource(string resourceName, Assembly assembly)
    {
        var resourcePath = resourceName.Replace('/', '.').Replace('\\', '.');

        var fullResourceName = assembly.GetManifestResourceNames()
                                       .FirstOrDefault(name => name.EndsWith(resourcePath, StringComparison.Ordinal));

        if (fullResourceName == null)
        {
            throw new FileNotFoundException($"Resource {resourceName} not found in assembly {assembly.FullName}");
        }

        using var stream = assembly.GetManifestResourceStream(fullResourceName);

        if (stream == null)
        {
            throw new InvalidOperationException(
                $"Unable to open resource stream for {resourceName} in assembly {assembly.FullName}"
            );
        }

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    [GeneratedRegex(@"\.([^\.]+)$")]
    private static partial Regex FileNameRegex();
}
