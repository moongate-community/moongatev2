namespace Moongate.Core.Utils;

/// <summary>
/// Utility methods for working with directories and file system operations
/// </summary>
public class DirectoriesUtils
{
    /// <summary>
    /// Gets files from the specified path recursively with optional extension filtering
    /// </summary>
    /// <param name="path">Directory path to search</param>
    /// <param name="extensions">File extensions to filter by (e.g., "*.txt", "*.json")</param>
    /// <returns>Array of file paths matching the criteria</returns>
    public static string[] GetFiles(string path, params string[] extensions)
        => GetFiles(path, true, extensions);

    /// <summary>
    /// Gets files from the specified path with configurable recursion and extension filtering
    /// </summary>
    /// <param name="path">Directory path to search</param>
    /// <param name="recursive">Whether to search subdirectories recursively</param>
    /// <param name="extensions">File extensions to filter by (e.g., "*.txt", "*.json")</param>
    /// <returns>Array of file paths matching the criteria</returns>
    public static string[] GetFiles(string path, bool recursive, params string[] extensions)
    {
        if (!Directory.Exists(path))
        {
            return [];
        }

        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = new List<string>();

        if (extensions == null || extensions.Length == 0)
        {
            return Directory.GetFiles(path, "*", searchOption);
        }

        foreach (var extension in extensions)
        {
            files.AddRange(Directory.GetFiles(path, extension, searchOption));
        }

        return files.ToArray();
    }
}
