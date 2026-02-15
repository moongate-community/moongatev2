using System.Collections;

namespace Moongate.Core.Extensions.Env;

/// <summary>
/// Provides extension methods for expanding environment variables in strings
/// </summary>
public static class EnvExtensions
{
    /// <summary>
    /// Expands environment variables in a string using custom $VARIABLE syntax
    /// </summary>
    /// <param name="input">The input string containing environment variable references</param>
    /// <returns>The string with environment variables expanded to their values</returns>
    public static string ExpandEnvironmentVariables(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
        {
            var key = $"${env.Key}";
            var value = env.Value?.ToString() ?? "";
            input = input.Replace(key, value);
        }

        return input;
    }
}
