/*************************************************************************
 * ModernUO                                                              *
 * Copyright 2019-2023 - ModernUO Development Team                       *
 * Email: hi@modernuo.com                                                *
 * File: Point2D.cs                                                      *
 *                                                                       *
 * This program is free software: you can redistribute it and/or modify  *
 * it under the terms of the GNU General Public License as published by  *
 * the Free Software Foundation, either version 3 of the License, or     *
 * (at your option) any later version.                                   *
 *                                                                       *
 * You should have received a copy of the GNU General Public License     *
 * along with this program.  If not, see <http://www.gnu.org/licenses/>. *
 *************************************************************************/

using System.Runtime.CompilerServices;
using Moongate.Core.Extensions.Strings;
using Moongate.UO.Data.Interfaces.Geometry;

namespace Moongate.UO.Data.Geometry;

public struct Point2D
    : IPoint2D, IComparable<Point2D>, IComparable<IPoint2D>, IEquatable<object>, IEquatable<Point2D>,
      IEquatable<IPoint2D>, ISpanFormattable, ISpanParsable<Point2D>
{
    public int X { get; set; }
    public int Y { get; set; }

    public static readonly Point2D Zero = new(0, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point2D(IPoint2D p) : this(p.X, p.Y) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point2D(Point3D p) : this(p.X, p.Y) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point2D(Point2D p) : this(p.X, p.Y) { }

    public Point2D(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int CompareTo(Point2D other)
    {
        var xComparison = X.CompareTo(other.X);

        return xComparison != 0 ? xComparison : Y.CompareTo(other.Y);
    }

    public int CompareTo(IPoint2D other)
    {
        var xComparison = X.CompareTo(other.X);

        return xComparison != 0 ? xComparison : Y.CompareTo(other.Y);
    }

    public bool Equals(Point2D other)
        => X == other.X && Y == other.Y;

    public bool Equals(IPoint2D other)
        => X == other?.X && Y == other.Y;

    public override bool Equals(object obj)
        => obj is Point2D other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(X, Y);

    public static bool operator ==(Point2D l, Point2D r)
        => l.X == r.X && l.Y == r.Y;

    public static bool operator ==(Point2D l, IPoint2D r)
        => !ReferenceEquals(r, null) && l.X == r.X && l.Y == r.Y;

    public static bool operator >(Point2D l, Point2D r)
        => l.X > r.X && l.Y > r.Y;

    public static bool operator >(Point2D l, IPoint2D r)
        => !ReferenceEquals(r, null) && l.X > r.X && l.Y > r.Y;

    public static bool operator >=(Point2D l, Point2D r)
        => l.X >= r.X && l.Y >= r.Y;

    public static bool operator >=(Point2D l, IPoint2D r)
        => !ReferenceEquals(r, null) && l.X >= r.X && l.Y >= r.Y;

    public static bool operator !=(Point2D l, Point2D r)
        => l.X != r.X || l.Y != r.Y;

    public static bool operator !=(Point2D l, IPoint2D r)
        => !ReferenceEquals(r, null) && (l.X != r.X || l.Y != r.Y);

    public static bool operator <(Point2D l, Point2D r)
        => l.X < r.X && l.Y < r.Y;

    public static bool operator <(Point2D l, IPoint2D r)
        => !ReferenceEquals(r, null) && l.X < r.X && l.Y < r.Y;

    public static bool operator <=(Point2D l, Point2D r)
        => l.X <= r.X && l.Y <= r.Y;

    public static bool operator <=(Point2D l, IPoint2D r)
        => !ReferenceEquals(r, null) && l.X <= r.X && l.Y <= r.Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2D Parse(string s)
        => Parse(s, null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2D Parse(string s, IFormatProvider provider)
        => Parse(s.AsSpan(), provider);

    public static Point2D Parse(ReadOnlySpan<char> s, IFormatProvider provider)
    {
        s = s.Trim();

        if (!s.StartsWithOrdinal('(') || !s.EndsWithOrdinal(')'))
        {
            throw new FormatException($"The input string '{s}' was not in a correct format.");
        }

        var comma = s.IndexOfOrdinal(',');

        if (comma == -1)
        {
            throw new FormatException($"The input string '{s}' was not in a correct format.");
        }

        var first = s.Slice(1, comma - 1).Trim();

        if (!int.TryParse(first, out var x))
        {
            throw new FormatException($"The input string '{s}' was not in a correct format.");
        }

        var second = s.Slice(comma + 1, s.Length - comma - 2).Trim();

        if (!int.TryParse(second, out var y))
        {
            throw new FormatException($"The input string '{s}' was not in a correct format.");
        }

        return new(x, y);
    }

    public override string ToString()
    {
        // Maximum number of characters that are needed to represent this:
        // 4 characters for (, )
        // Up to 11 characters to represent each integer
        const int maxLength = 4 + 11 * 2;
        Span<char> span = stackalloc char[maxLength];
        TryFormat(span, out var charsWritten, null, null);

        return span[..charsWritten].ToString();
    }

    public string ToString(string format, IFormatProvider formatProvider)

        // format and formatProvider are not doing anything right now, so use the
        // default ToString implementation.
        => ToString();

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
        => destination.TryWrite(provider, $"({X}, {Y})", out charsWritten);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(string s, IFormatProvider provider, out Point2D result)
        => TryParse(s.AsSpan(), provider, out result);

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out Point2D result)
    {
        s = s.Trim();

        if (!s.StartsWithOrdinal('(') || !s.EndsWithOrdinal(')'))
        {
            result = default;

            return false;
        }

        var comma = s.IndexOfOrdinal(',');

        if (comma == -1)
        {
            result = default;

            return false;
        }

        var first = s.Slice(1, comma - 1).Trim();

        if (!int.TryParse(first, out var x))
        {
            result = default;

            return false;
        }

        var second = s.Slice(comma + 1, s.Length - comma - 2).Trim();

        if (!int.TryParse(second, out var y))
        {
            result = default;

            return false;
        }

        result = new(x, y);

        return true;
    }
}
