using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Moongate.Network.Spans;

public ref struct SpanWriter : IDisposable
{
    private readonly bool _resize;
    private byte[]? _arrayToReturnToPool;
    private Span<byte> _buffer;
    private int _position;

    public const int AttributeMaximum = 100;

    public int BytesWritten { get; private set; }

    public int Position
    {
        get => _position;
        private set
        {
            _position = value;

            if (value > BytesWritten)
            {
                BytesWritten = value;
            }
        }
    }

    public int Capacity => _buffer.Length;
    public ReadOnlySpan<byte> Span => _buffer[..Position];
    public Span<byte> RawBuffer => _buffer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SpanWriter(Span<byte> initialBuffer, bool resize = false)
    {
        _resize = resize;
        _buffer = initialBuffer;
        _position = 0;
        BytesWritten = 0;
        _arrayToReturnToPool = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SpanWriter(int initialCapacity, bool resize = false)
    {
        _resize = resize;
        _arrayToReturnToPool = ArrayPool<byte>.Shared.Rent(initialCapacity);
        _buffer = _arrayToReturnToPool;
        _position = 0;
        BytesWritten = 0;
    }

    public struct SpanOwner : IDisposable
    {
        private readonly int _length;
        private readonly byte[]? _buffer;
        private readonly bool _isPooled;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal SpanOwner(int length, byte[]? buffer, bool isPooled)
        {
            _length = length;
            _buffer = buffer;
            _isPooled = isPooled;
        }

        public Span<byte> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer is null ? Span<byte>.Empty : _buffer.AsSpan(0, _length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_isPooled && _buffer is not null)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear(int count)
    {
        GrowIfNeeded(count);
        _buffer.Slice(_position, count).Clear();
        Position += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        var toReturn = _arrayToReturnToPool;
        this = default;

        if (toReturn is not null)
        {
            ArrayPool<byte>.Shared.Return(toReturn);
        }
    }

    public void EnsureCapacity(int capacity)
    {
        if (capacity > _buffer.Length)
        {
            if (!_resize)
            {
                throw new InvalidOperationException("Insufficient capacity and resizing is disabled.");
            }

            Grow(capacity - BytesWritten);
        }
    }

    public ref byte GetPinnableReference()
        => ref MemoryMarshal.GetReference(_buffer);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Grow(int additionalCapacity)
    {
        var newSize = Math.Max(BytesWritten + additionalCapacity, _buffer.Length * 2);
        var poolArray = ArrayPool<byte>.Shared.Rent(newSize);

        _buffer[..BytesWritten].CopyTo(poolArray);

        var toReturn = _arrayToReturnToPool;
        _buffer = _arrayToReturnToPool = poolArray;

        if (toReturn is not null)
        {
            ArrayPool<byte>.Shared.Return(toReturn);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Seek(int offset, SeekOrigin origin)
    {
        var newPosition = Math.Max(
            0,
            origin switch
            {
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End     => BytesWritten + offset,
                _                  => offset
            }
        );

        if (newPosition > _buffer.Length)
        {
            if (!_resize)
            {
                throw new IOException("Attempted to seek beyond the available capacity.");
            }

            Grow(newPosition - _buffer.Length + 1);
        }

        Position = newPosition;

        return Position;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ToArray()
    {
        if (_position == 0)
        {
            return Array.Empty<byte>();
        }

        var result = new byte[_position];
        _buffer[.._position].CopyTo(result);

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SpanOwner ToSpan()
    {
        if (_position == 0)
        {
            var toReturn = _arrayToReturnToPool;
            this = default;

            if (toReturn is not null)
            {
                ArrayPool<byte>.Shared.Return(toReturn);
            }

            return new(0, null, false);
        }

        var currentPoolBuffer = _arrayToReturnToPool;

        if (currentPoolBuffer is not null)
        {
            this = default;

            return new(_position, currentPoolBuffer, true);
        }

        var ownedBuffer = ArrayPool<byte>.Shared.Rent(_position);
        _buffer[.._position].CopyTo(ownedBuffer);
        this = default;

        return new(_position, ownedBuffer, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(bool value)
    {
        GrowIfNeeded(1);
        _buffer[Position++] = value ? (byte)1 : (byte)0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(byte value)
    {
        GrowIfNeeded(1);
        _buffer[Position++] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(sbyte value)
    {
        GrowIfNeeded(1);
        _buffer[Position++] = (byte)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(short value)
    {
        GrowIfNeeded(2);
        BinaryPrimitives.WriteInt16BigEndian(_buffer[_position..], value);
        Position += 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ushort value)
    {
        GrowIfNeeded(2);
        BinaryPrimitives.WriteUInt16BigEndian(_buffer[_position..], value);
        Position += 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(int value)
    {
        GrowIfNeeded(4);
        BinaryPrimitives.WriteInt32BigEndian(_buffer[_position..], value);
        Position += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(uint value)
    {
        GrowIfNeeded(4);
        BinaryPrimitives.WriteUInt32BigEndian(_buffer[_position..], value);
        Position += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(long value)
    {
        GrowIfNeeded(8);
        BinaryPrimitives.WriteInt64BigEndian(_buffer[_position..], value);
        Position += 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ulong value)
    {
        GrowIfNeeded(8);
        BinaryPrimitives.WriteUInt64BigEndian(_buffer[_position..], value);
        Position += 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ReadOnlySpan<byte> buffer)
    {
        var count = buffer.Length;
        GrowIfNeeded(count);
        buffer.CopyTo(_buffer[_position..]);
        Position += count;
    }

    public void Write(ReadOnlySpan<char> value, Encoding encoding, int fixedLength = -1)
    {
        var charLength = Math.Min(fixedLength > -1 ? fixedLength : value.Length, value.Length);
        var src = value[..charLength];

        var byteLength = GetTerminatorWidth(encoding);
        var byteCount = encoding.GetByteCount(src);

        if (fixedLength > src.Length)
        {
            byteCount += (fixedLength - src.Length) * byteLength;
        }

        if (byteCount == 0)
        {
            return;
        }

        GrowIfNeeded(byteCount);
        var bytesWritten = encoding.GetBytes(src, _buffer[_position..]);
        Position += bytesWritten;

        if (fixedLength > -1)
        {
            var extra = fixedLength * byteLength - bytesWritten;

            if (extra > 0)
            {
                Clear(extra);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteAscii(char chr)
        => Write((byte)chr);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteAscii(string value)
        => Write(value.AsSpan(), Encoding.ASCII);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteAscii(string value, int fixedLength)
        => Write(value.AsSpan(), Encoding.ASCII, fixedLength);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteAsciiNull(string value)
    {
        Write(value.AsSpan(), Encoding.ASCII);
        Write((byte)0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteAttribute(int max, int cur, bool normalize = false, bool reverse = false)
    {
        if (normalize && max != 0)
        {
            if (reverse)
            {
                Write((short)(cur * AttributeMaximum / max));
                Write((short)AttributeMaximum);
            }
            else
            {
                Write((short)AttributeMaximum);
                Write((short)(cur * AttributeMaximum / max));
            }
        }
        else
        {
            if (reverse)
            {
                Write((short)cur);
                Write((short)max);
            }
            else
            {
                Write((short)max);
                Write((short)cur);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBigUni(string value)
        => Write(value.AsSpan(), Encoding.BigEndianUnicode);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBigUni(string value, int fixedLength)
        => Write(value.AsSpan(), Encoding.BigEndianUnicode, fixedLength);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBigUniNull(string value)
    {
        Write(value.AsSpan(), Encoding.BigEndianUnicode);
        Write((ushort)0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLE(short value)
    {
        GrowIfNeeded(2);
        BinaryPrimitives.WriteInt16LittleEndian(_buffer[_position..], value);
        Position += 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLE(ushort value)
    {
        GrowIfNeeded(2);
        BinaryPrimitives.WriteUInt16LittleEndian(_buffer[_position..], value);
        Position += 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLE(int value)
    {
        GrowIfNeeded(4);
        BinaryPrimitives.WriteInt32LittleEndian(_buffer[_position..], value);
        Position += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLE(uint value)
    {
        GrowIfNeeded(4);
        BinaryPrimitives.WriteUInt32LittleEndian(_buffer[_position..], value);
        Position += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLittleUni(string value)
        => Write(value.AsSpan(), Encoding.Unicode);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLittleUni(string value, int fixedLength)
        => Write(value.AsSpan(), Encoding.Unicode, fixedLength);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLittleUniNull(string value)
    {
        Write(value.AsSpan(), Encoding.Unicode);
        Write((ushort)0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WritePacketLength()
    {
        Seek(1, SeekOrigin.Begin);
        Write((ushort)BytesWritten);
        Seek(0, SeekOrigin.End);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUTF8(string value)
        => Write(value.AsSpan(), Encoding.UTF8);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUTF8Null(string value)
    {
        Write(value.AsSpan(), Encoding.UTF8);
        Write((byte)0);
    }

    private static int GetTerminatorWidth(Encoding encoding)
        => ReferenceEquals(encoding, Encoding.Unicode) || ReferenceEquals(encoding, Encoding.BigEndianUnicode) ? 2 : 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GrowIfNeeded(int count)
    {
        if (_position + count <= _buffer.Length)
        {
            return;
        }

        if (!_resize)
        {
            throw new InvalidOperationException("Insufficient capacity and resizing is disabled.");
        }

        Grow(count);
    }
}
