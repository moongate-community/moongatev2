using System.Runtime.CompilerServices;

namespace Moongate.UO.Data.Ids;

/// <summary>
/// Represents a UO entity serial identifier.
/// </summary>
public readonly struct Serial
    : IComparable<Serial>, IComparable<uint>, IEquatable<Serial>, ISpanFormattable, ISpanParsable<Serial>
{
    public const uint ItemOffset = 0x40000000;
    public const uint MaxItemSerial = 0x7EEEEEEE;
    public const uint MaxMobileSerial = ItemOffset - 1;
    public const int MobileStart = 0x00000001;

    public static readonly Serial ItemOffsetSerial = new(ItemOffset);
    public static readonly Serial MinusOne = new(0xFFFFFFFF);
    public static readonly Serial Zero = new(0);

    public Serial(uint serial)
        => Value = serial;

    public uint Value { get; }

    public bool IsMobile
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value is > 0 and < ItemOffset;
    }

    public bool IsItem
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value is >= ItemOffset and < MaxItemSerial;
    }

    public bool IsValid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Serial other)
        => Value.CompareTo(other.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(uint other)
        => Value.CompareTo(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Serial other)
        => Value == other.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
        => obj switch
        {
            Serial serial => this == serial,
            uint raw => Value == raw,
            _ => false
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
        => Value.GetHashCode();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ToInt32()
        => (int)Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Serial RandomSerial()
        => new((uint)Random.Shared.Next(1, int.MaxValue));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Serial Parse(string s)
        => Parse(s, null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Serial Parse(string s, IFormatProvider? provider)
        => Parse(s.AsSpan(), provider);

    public static Serial Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out var serial))
        {
            return serial;
        }

        throw new FormatException("Input string was not in a correct serial format.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(string? s, IFormatProvider? provider, out Serial result)
        => TryParse(s.AsSpan(), provider, out result);

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Serial result)
    {
        _ = provider;

        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            s = s[2..];

            if (uint.TryParse(s, System.Globalization.NumberStyles.HexNumber, null, out var hexValue))
            {
                result = new(hexValue);
                return true;
            }
        }

        if (uint.TryParse(s, out var value))
        {
            result = new(value);
            return true;
        }

        result = default;
        return false;
    }

    public override string ToString()
    {
        Span<char> destination = stackalloc char[10];
        TryFormat(destination, out var charsWritten, default, null);

        return destination[..charsWritten].ToString();
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
        => ToString();

    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider
    )
        => format != ReadOnlySpan<char>.Empty
               ? Value.TryFormat(destination, out charsWritten, format, provider)
               : destination.TryWrite(provider, $"0x{Value:X8}", out charsWritten);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator uint(Serial value)
        => value.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Serial(uint value)
        => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Serial operator +(Serial left, Serial right)
        => (Serial)(left.Value + right.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Serial operator +(Serial left, uint right)
        => (Serial)(left.Value + right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Serial operator -(Serial left, Serial right)
        => (Serial)(left.Value - right.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Serial operator -(Serial left, uint right)
        => (Serial)(left.Value - right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Serial operator ++(Serial value)
        => (Serial)(value.Value + 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Serial operator --(Serial value)
        => (Serial)(value.Value - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Serial left, Serial right)
        => left.Value == right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Serial left, Serial right)
        => left.Value != right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Serial left, uint right)
        => left.Value == right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Serial left, uint right)
        => left.Value != right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Serial left, Serial right)
        => left.Value < right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Serial left, Serial right)
        => left.Value <= right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Serial left, Serial right)
        => left.Value > right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Serial left, Serial right)
        => left.Value >= right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Serial left, uint right)
        => left.Value < right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Serial left, uint right)
        => left.Value <= right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Serial left, uint right)
        => left.Value > right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Serial left, uint right)
        => left.Value >= right;
}
