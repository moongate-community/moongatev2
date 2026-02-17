using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Moongate.Core.Buffers;

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

public ref struct ValueStringBuilder
{
    private char[] _arrayToReturnToPool;
    private Span<char> _chars;
    private readonly bool _mt;

    private ArrayPool<char> ArrayPool
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _mt ? ArrayPool<char>.Shared : STArrayPool<char>.Shared;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueStringBuilder Create(int capacity = 64, bool mt = false)
        => new(capacity, mt);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueStringBuilder CreateMT(int capacity = 64)
        => new(capacity, true);

    // If this ctor is used, you cannot pass in stackalloc ROS for append/replace.
    public ValueStringBuilder(ReadOnlySpan<char> initialString, bool mt = false) : this(initialString.Length, mt)
    {
        Append(initialString);
    }

    public ValueStringBuilder(ReadOnlySpan<char> initialString, Span<char> initialBuffer, bool mt = false) : this(
        initialBuffer,
        mt
    )
    {
        Append(initialString);
    }

    public ValueStringBuilder(Span<char> initialBuffer, bool mt = false)
    {
        _mt = mt;
        _arrayToReturnToPool = null;
        _chars = initialBuffer;
        Length = 0;
    }

    // If this ctor is used, you cannot pass in stackalloc ROS for append/replace.
    public ValueStringBuilder(int initialCapacity, bool mt = false)
    {
        _mt = mt;
        Length = 0;
        _arrayToReturnToPool = (_mt ? ArrayPool<char>.Shared : STArrayPool<char>.Shared).Rent(initialCapacity);
        _chars = _arrayToReturnToPool;
    }

    public int Length { get; private set; }

    public int Capacity => _chars.Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
        Length = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCapacity(int capacity)
    {
        if (capacity > _chars.Length)
        {
            Grow(capacity - Length);
        }
    }

    /// <summary>
    /// Get a pinnable reference to the builder.
    /// Does not ensure there is a null char after <see cref="Length" />
    /// This overload is pattern matched in the C# 7.3+ compiler so you can omit
    /// the explicit method call, and write eg "fixed (char* c = builder)"
    /// </summary>
    public ref char GetPinnableReference()
        => ref MemoryMarshal.GetReference(_chars);

    /// <summary>
    /// Get a pinnable reference to the builder.
    /// </summary>
    /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length" /></param>
    public ref char GetPinnableReference(bool terminate)
    {
        if (terminate)
        {
            EnsureCapacity(Length + 1);
            _chars[Length] = '\0';
        }

        return ref MemoryMarshal.GetReference(_chars);
    }

    public ref char this[int index] => ref _chars[index];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
        => _chars[..Length].ToString();

    /// <summary>Returns the underlying storage of the builder.</summary>
    public Span<char> RawChars => _chars;

    /// <summary>
    /// Returns a span around the contents of the builder.
    /// </summary>
    /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length" /></param>
    public ReadOnlySpan<char> AsSpan(bool terminate)
    {
        if (terminate)
        {
            EnsureCapacity(Length + 1);
            _chars[Length] = '\0';
        }

        return _chars[..Length];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> AsSpan()
        => _chars[..Length];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> AsSpan(int start)
        => _chars[start..];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> AsSpan(int start, int length)
        => _chars.Slice(start, length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryCopyTo(Span<char> destination, out int charsWritten)
    {
        if (_chars[..Length].TryCopyTo(destination))
        {
            charsWritten = Length;

            return true;
        }

        charsWritten = 0;

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert(int index, char value, int count)
    {
        if (Length > _chars.Length - count)
        {
            Grow(count);
        }

        var remaining = Length - index;
        _chars.Slice(index, remaining).CopyTo(_chars[(index + count)..]);
        _chars.Slice(index, count).Fill(value);
        Length += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert(int index, string? s)
    {
        if (s == null)
        {
            return;
        }

        var count = s.Length;

        if (Length > _chars.Length - count)
        {
            Grow(count);
        }

        var remaining = Length - index;
        _chars.Slice(index, remaining).CopyTo(_chars[(index + count)..]);
        s.AsSpan().CopyTo(_chars[index..]);
        Length += count;
    }

    public void Append<T>(T value, string? format = null)
    {
        if (value is IFormattable)
        {
            if (value is ISpanFormattable)
            {
                var destination = _chars[Length..];
                int charsWritten;

                while (!((ISpanFormattable)value).TryFormat(destination, out charsWritten, format, default))
                {
                    Grow(1);
                    destination = _chars[Length..];
                }

                if ((uint)charsWritten > (uint)destination.Length)
                {
                    throw new FormatException("Invalid string");
                }

                Length += charsWritten;
            }
            else
            {
                Append(((IFormattable)value).ToString(format, default)); // constrained call avoiding boxing for value types
            }
        }
        else if (value is not null)
        {
            Append(value.ToString());
        }
    }

    // Compiler generated
    public void Append(ref RawInterpolatedStringHandler handler)
        => Append(handler.Text);

    // Compiler generated
    public void Append(
        IFormatProvider? formatProvider,
        [InterpolatedStringHandlerArgument("formatProvider")]
        ref RawInterpolatedStringHandler handler
    )
        => Append(handler.Text);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(string? s)
    {
        if (s == null)
        {
            return;
        }

        var pos = Length;

        if (s.Length == 1 &&
            (uint)pos <
            (uint)_chars.Length) // very common case, e.g. appending strings from NumberFormatInfo like separators, percent symbols, etc.
        {
            _chars[pos] = s[0];
            Length = pos + 1;
        }
        else
        {
            AppendSlow(s);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLine(string? s)
    {
        if (s == null)
        {
            return;
        }

        // very common case, e.g. appending strings from NumberFormatInfo like separators, percent symbols, etc.
        if (s.Length == 1)
        {
            Append(s[0]);
        }
        else
        {
            AppendSlow(s);
        }

        Append(Environment.NewLine);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AppendSlow(string? s)
    {
        var pos = Length;

        if (pos > _chars.Length - s.Length)
        {
            Grow(s.Length);
        }

        s.AsSpan().CopyTo(_chars[pos..]);
        Length += s.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, int count)
    {
        if (Length > _chars.Length - count)
        {
            Grow(count);
        }

        var dst = _chars.Slice(Length, count);

        for (var i = 0; i < dst.Length; i++)
        {
            dst[i] = c;
        }
        Length += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void Append(char* value, int length)
    {
        var pos = Length;

        if (pos > _chars.Length - length)
        {
            Grow(length);
        }

        var dst = _chars.Slice(Length, length);

        for (var i = 0; i < dst.Length; i++)
        {
            dst[i] = *value++;
        }
        Length += length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> value)
    {
        var pos = Length;

        if (pos > _chars.Length - value.Length)
        {
            Grow(value.Length);
        }

        value.CopyTo(_chars[Length..]);
        Length += value.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<char> AppendSpan(int length)
    {
        var origPos = Length;

        if (origPos > _chars.Length - length)
        {
            Grow(length);
        }

        Length = origPos + length;

        return _chars.Slice(origPos, length);
    }

    /// <summary>
    /// Resize the internal buffer either by doubling current buffer size or
    /// by adding <paramref name="additionalCapacityBeyondPos" /> to
    /// <see cref="Length" /> whichever is greater.
    /// </summary>
    /// <param name="additionalCapacityBeyondPos">
    /// Number of chars requested beyond current position.
    /// </param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int additionalCapacityBeyondPos)
    {
        var poolArray = ArrayPool.Rent(Math.Max(Length + additionalCapacityBeyondPos, _chars.Length * 2));

        _chars[..Length].CopyTo(poolArray);

        var toReturn = _arrayToReturnToPool;
        _chars = _arrayToReturnToPool = poolArray;

        if (toReturn != null)
        {
            ArrayPool.Return(toReturn);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (_arrayToReturnToPool != null)
        {
            ArrayPool.Return(_arrayToReturnToPool);
        }

        this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(int startIndex, int length)
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        if (startIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        }

        if (length > Length - startIndex)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        if (startIndex == 0)
        {
            _chars = _chars[length..];
        }
        else if (startIndex + length == Length)
        {
            _chars = _chars[..startIndex];
        }
        else
        {
            // Somewhere in the middle, this will be slow
            _chars[(startIndex + length)..].CopyTo(_chars[startIndex..]);
        }

        Length -= length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Replace(char oldChar, char newChar, int startIndex, int count)
    {
        var currentLength = Length;

        if ((uint)startIndex > (uint)currentLength)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        }

        if (count < 0 || startIndex > currentLength - count)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        var slice = _chars;

        while (true)
        {
            var indexOf = slice.IndexOf(oldChar);

            if (indexOf == -1)
            {
                break;
            }

            slice[indexOf] = newChar;
            slice = slice[(indexOf + 1)..];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReplaceAny(ReadOnlySpan<char> oldChars, ReadOnlySpan<char> newChars, int startIndex, int count)
    {
        var currentLength = Length;

        if ((uint)startIndex > (uint)currentLength)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        }

        if (count < 0 || startIndex > currentLength - count)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        var slice = _chars;

        while (true)
        {
            var indexOf = slice.IndexOfAny(oldChars);

            if (indexOf == -1)
            {
                break;
            }

            var chr = slice[indexOf];

            slice[indexOf] = newChars[oldChars.IndexOf(chr)];
            slice = slice[(indexOf + 1)..];
        }
    }
}
