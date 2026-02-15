using DarkLilly.Core.Extensions.Env;

namespace DarkLilly.Core.Extensions.Directories;

/// <summary>
/// Provides extension methods for directory path resolution and environment variable expansion
/// </summary>
public static class DirectoriesExtension
{
    /// <summary>
    /// Resolves path by expanding tilde (~) to user home directory and expanding environment variables
    /// </summary>
    /// <param name="path">The path to resolve</param>
    /// <returns>The fully resolved path with expanded environment variables</returns>
    /// <exception cref="ArgumentException">Thrown when path is null or empty</exception>
    public static string ResolvePathAndEnvs(this string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        path = path.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

        path = Environment.ExpandEnvironmentVariables(path).ExpandEnvironmentVariables();

        return path;
    }
}
