using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Moongate.Core.Utils;

/// <summary>
/// Provides utility methods for string operations, including various case conversion methods.
/// </summary>
public static partial class StringUtils
{
    private static readonly Regex WordSplitterRegex = WordSplitter();

    /// <summary>
    /// Converts a string to camelCase.
    /// </summary>
    /// <param name="text">The string to convert to camelCase.</param>
    /// <returns>A camelCase version of the input string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input text is null or empty.</exception>
    /// <example>
    /// "HelloWorld" becomes "helloWorld"
    /// "API_RESPONSE" becomes "apiResponse"
    /// "user-id" becomes "userId"
    /// </example>
    public static string ToCamelCase(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (text.Length < 2)
        {
            return text.ToLowerInvariant();
        }

        var words = WordSplitterRegex.Split(text);
        var result = new StringBuilder(words[0].ToLowerInvariant());

        for (var i = 1; i < words.Length; i++)
        {
            if (string.IsNullOrEmpty(words[i]))
            {
                continue;
            }

            result.Append(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(words[i].ToLowerInvariant()));
        }

        return result.ToString();
    }

    /// <summary>
    /// Converts a string to Dot Case.
    /// </summary>
    /// <param name="text">The string to convert to Dot Case.</param>
    /// <returns>A Dot Case version of the input string.</returns>
    /// <example>
    /// "HelloWorld" becomes "hello.world"
    /// "API_RESPONSE" becomes "api.response"
    /// </example>
    public static string ToDotCase(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (text.Length < 2)
        {
            return text.ToLowerInvariant();
        }

        var words = WordSplitterRegex.Split(text);
        var result = new StringBuilder();

        var isFirst = true;

        foreach (var word in words)
        {
            if (string.IsNullOrEmpty(word))
            {
                continue;
            }

            if (!isFirst)
            {
                result.Append('.');
            }

            result.Append(word.ToLowerInvariant());
            isFirst = false;
        }

        return result.ToString();
    }

    /// <summary>
    /// Converts a string to kebab-case.
    /// </summary>
    /// <param name="text">The string to convert to kebab-case.</param>
    /// <returns>A kebab-case version of the input string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input text is null or empty.</exception>
    /// <example>
    /// "HelloWorld" becomes "hello-world"
    /// "API_RESPONSE" becomes "api-response"
    /// "userId" becomes "user-id"
    /// </example>
    public static string ToKebabCase(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (text.Length < 2)
        {
            return text.ToLowerInvariant();
        }

        var words = WordSplitterRegex.Split(text);
        var result = new StringBuilder();

        var isFirst = true;

        foreach (var word in words)
        {
            if (string.IsNullOrEmpty(word))
            {
                continue;
            }

            if (!isFirst)
            {
                result.Append('-');
            }

            result.Append(word.ToLowerInvariant());
            isFirst = false;
        }

        return result.ToString();
    }

    /// <summary>
    /// Converts a string to PascalCase.
    /// </summary>
    /// <param name="text">The string to convert to PascalCase.</param>
    /// <returns>A PascalCase version of the input string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input text is null or empty.</exception>
    /// <example>
    /// "hello_world" becomes "HelloWorld"
    /// "api-response" becomes "ApiResponse"
    /// "userId" becomes "UserId"
    /// </example>
    public static string ToPascalCase(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (text.Length < 2)
        {
            return text.ToUpperInvariant();
        }

        var words = WordSplitterRegex.Split(text);
        var result = new StringBuilder();

        foreach (var word in words)
        {
            if (string.IsNullOrEmpty(word))
            {
                continue;
            }

            result.Append(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(word.ToLowerInvariant()));
        }

        return result.ToString();
    }

    /// <summary>
    /// Converts a string to Path Case.
    /// </summary>
    /// <param name="text">The string to convert to Path Case.</param>
    /// <returns>A Path Case version of the input string.</returns>
    /// <example>
    /// "HelloWorld" becomes "hello/world"
    /// "API_RESPONSE" becomes "api/response"
    /// </example>
    public static string ToPathCase(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (text.Length < 2)
        {
            return text.ToLowerInvariant();
        }

        var words = WordSplitterRegex.Split(text);
        var result = new StringBuilder();

        var isFirst = true;

        foreach (var word in words)
        {
            if (string.IsNullOrEmpty(word))
            {
                continue;
            }

            if (!isFirst)
            {
                result.Append('/');
            }

            result.Append(word.ToLowerInvariant());
            isFirst = false;
        }

        return result.ToString();
    }

    /// <summary>
    /// Converts a string to Sentence Case.
    /// </summary>
    /// <param name="text">The string to convert to Sentence Case.</param>
    /// <returns>A Sentence Case version of the input string.</returns>
    /// <example>
    /// "hello world" becomes "Hello world"
    /// "API_RESPONSE" becomes "Api response"
    /// </example>
    public static string ToSentenceCase(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (text.Length < 2)
        {
            return text.ToUpperInvariant();
        }

        // Split only on spaces, underscores, and hyphens, not camelCase
        var simpleWords = Regex.Split(text, @"[\s_-]+").Where(w => !string.IsNullOrEmpty(w)).ToArray();

        if (simpleWords.Length == 1)
        {
            // Single word (possibly camelCase): capitalize first letter, lowercase the rest
            var word = simpleWords[0];

            return char.ToUpperInvariant(word[0]) + word.Substring(1).ToLowerInvariant();
        }

        // Multiple words: capitalize first word, lowercase the rest
        var result = new StringBuilder();

        for (var i = 0; i < simpleWords.Length; i++)
        {
            if (i > 0)
            {
                result.Append(' ');
            }

            var word = simpleWords[i];

            if (i == 0)
            {
                result.Append(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(word.ToLowerInvariant()));
            }
            else
            {
                result.Append(word.ToLowerInvariant());
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Converts a string from camelCase or PascalCase to snake_case.
    /// </summary>
    /// <param name="text">The string to convert to snake_case.</param>
    /// <returns>A snake_case version of the input string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input text is null or empty.</exception>
    /// <example>
    /// "HelloWorld" becomes "hello_world"
    /// "APIResponse" becomes "api_response"
    /// "userId" becomes "user_id"
    /// </example>
    public static string ToSnakeCase(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (text.Length < 2)
        {
            return text.ToLowerInvariant();
        }

        var words = WordSplitterRegex.Split(text);
        var result = new StringBuilder();

        var isFirst = true;

        foreach (var word in words)
        {
            if (string.IsNullOrEmpty(word))
            {
                continue;
            }

            if (!isFirst)
            {
                result.Append('_');
            }

            result.Append(word.ToLowerInvariant());
            isFirst = false;
        }

        return result.ToString();
    }

    /// <summary>
    /// Converts a string to Title Case.
    /// </summary>
    /// <param name="text">The string to convert to Title Case.</param>
    /// <returns>A Title Case version of the input string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input text is null or empty.</exception>
    /// <example>
    /// "hello_world" becomes "Hello World"
    /// "API_RESPONSE" becomes "Api Response"
    /// "user-id" becomes "User Id"
    /// </example>
    public static string ToTitleCase(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var words = WordSplitterRegex.Split(text);
        var result = new StringBuilder();

        var isFirst = true;

        foreach (var word in words)
        {
            if (string.IsNullOrEmpty(word))
            {
                continue;
            }

            if (!isFirst)
            {
                result.Append(' ');
            }

            result.Append(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(word.ToLowerInvariant()));
            isFirst = false;
        }

        return result.ToString();
    }

    /// <summary>
    /// Converts a string to Train Case (Pascal Case with hyphens).
    /// </summary>
    /// <param name="text">The string to convert to Train Case.</param>
    /// <returns>A Train Case version of the input string.</returns>
    /// <example>
    /// "hello_world" becomes "Hello-World"
    /// "apiResponse" becomes "Api-Response"
    /// </example>
    public static string ToTrainCase(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (text.Length < 2)
        {
            return text.ToUpperInvariant();
        }

        var words = WordSplitterRegex.Split(text);
        var result = new StringBuilder();

        var isFirst = true;

        foreach (var word in words)
        {
            if (string.IsNullOrEmpty(word))
            {
                continue;
            }

            if (!isFirst)
            {
                result.Append('-');
            }

            result.Append(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(word.ToLowerInvariant()));
            isFirst = false;
        }

        return result.ToString();
    }

    /// <summary>
    /// Converts a string to UPPER_SNAKE_CASE (screaming snake case).
    /// </summary>
    /// <param name="text">The string to convert to UPPER_SNAKE_CASE.</param>
    /// <returns>An UPPER_SNAKE_CASE version of the input string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input text is null or empty.</exception>
    /// <example>
    /// "HelloWorld" becomes "HELLO_WORLD"
    /// "apiResponse" becomes "API_RESPONSE"
    /// "user-id" becomes "USER_ID"
    /// </example>
    public static string ToUpperSnakeCase(string text)
        => ToSnakeCase(text).ToUpperInvariant();

    [GeneratedRegex(@"[\s_-]|(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", RegexOptions.Compiled)]
    private static partial Regex WordSplitter();
}
