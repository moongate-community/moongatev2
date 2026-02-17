using System.Runtime.CompilerServices;

namespace Moongate.Core.Extensions.Strings;

public static class OrdinalStringHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CompareOrdinal(this ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        => a.CompareTo(b, StringComparison.Ordinal);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CompareOrdinal(this string a, string b)
        => string.CompareOrdinal(a, b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsOrdinal(this ReadOnlySpan<char> a, string b)
        => a.Contains(b, StringComparison.Ordinal);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsOrdinal(this string a, string b)
        => a?.Contains(b, StringComparison.Ordinal) == true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsOrdinal(this ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        => a.Contains(b, StringComparison.Ordinal);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsOrdinal(this string a, char b)
        => a?.Contains(b, StringComparison.Ordinal) == true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EndsWithOrdinal(this string a, string b)
        => a?.EndsWith(b, StringComparison.Ordinal) == true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EndsWithOrdinal(this ReadOnlySpan<char> a, char b)
        => a.EndsWithOrdinal(new ReadOnlySpan<char>(ref b));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EndsWithOrdinal(this ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        => a.EndsWith(b, StringComparison.Ordinal);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsOrdinal(this ReadOnlySpan<char> a, string b)
        => a.Equals(b, StringComparison.Ordinal);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsOrdinal(this string a, string b)
        => a?.Equals(b, StringComparison.Ordinal) ?? b == null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfOrdinal(this string a, char b)
        => a?.IndexOf(b, StringComparison.Ordinal) ?? -1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfOrdinal(this string a, string b)
        => a?.IndexOf(b, StringComparison.Ordinal) ?? -1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfOrdinal(this string a, string b, int startIndex)
        => a?.IndexOf(b, startIndex, StringComparison.Ordinal) ?? -1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfOrdinal(this ReadOnlySpan<char> a, char b)
        => a.IndexOfOrdinal(new ReadOnlySpan<char>(ref b));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfOrdinal(this ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        => a.IndexOf(b, StringComparison.Ordinal);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string RemoveOrdinal(this string a, string b)
        => a?.Replace(b, "", StringComparison.Ordinal);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RemoveOrdinal(this ReadOnlySpan<char> a, ReadOnlySpan<char> b, Span<char> buffer, out int size)
        => a.Remove(b, StringComparison.Ordinal, buffer, out size);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string RemoveOrdinal(this ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        => a.Remove(b, StringComparison.Ordinal);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ReplaceOrdinal(this string a, string o, string n)
        => a?.Replace(o, n, StringComparison.Ordinal);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWithOrdinal(this ReadOnlySpan<char> a, char b)
        => a.StartsWithOrdinal(new ReadOnlySpan<char>(ref b));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWithOrdinal(this ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        => a.StartsWith(b, StringComparison.Ordinal);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWithOrdinal(this string a, string b)
        => a?.StartsWith(b, StringComparison.Ordinal) == true;
}
