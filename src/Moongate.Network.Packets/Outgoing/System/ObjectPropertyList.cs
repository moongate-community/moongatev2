using System.Buffers;
using System.Globalization;
using System.Runtime.CompilerServices;
using Moongate.Abstractions.Interfaces;
using Moongate.Network.Packets.Base;
using Moongate.Network.Spans;
using Moongate.UO.Data.Ids;

namespace Moongate.Network.Packets.Outgoing.System;

/// <summary>
/// A zero-allocation implementation of the Mega Cliloc (0xD6) packet.
/// Implements IPropertyList to write cliloc IDs and string formats directly to a reusable buffer.
/// </summary>
public sealed class ObjectPropertyList : BaseGameNetworkPacket, IPropertyList, IDisposable
{
    private static readonly uint[] StringNumbers =
    {
        1042971, // ~1_NOTHING~
        1070722, // ~1_NOTHING~
        1114057, // ~1_val~
        1114778, // ~1_val~
        1114779  // ~1_val~
    };

    private byte[]? _buffer;
    private int _bufferPos;
    private int _hash;
    private int _stringNumbersIndex;
    
    public int Hash => 0x40000000 + _hash;
    public Serial Serial { get; }
    public uint Header { get; private set; }

    public ObjectPropertyList(Serial serial) : base(0xD6)
    {
        Serial = serial;
        _buffer = ArrayPool<byte>.Shared.Rent(512);

        var writer = new SpanWriter(_buffer);
        
        // Write subcommand 0x0001
        writer.Write((ushort)0x0001);
        writer.Write(Serial.Value);
        writer.Write((ushort)0x0000); // Unknown
        writer.Write(Serial.Value);

        _bufferPos = writer.Position;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCapacity(int requiredBytes)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(ObjectPropertyList));

        if (_bufferPos + requiredBytes > _buffer.Length)
        {
            var newSize = Math.Max(_buffer.Length * 2, _bufferPos + requiredBytes);
            var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
            
            _buffer.AsSpan(0, _bufferPos).CopyTo(newBuffer);
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = newBuffer;
        }
    }

    private void AddHash(int val)
    {
        _hash ^= val & 0x3FFFFFF;
        _hash ^= (val >> 26) & 0x3F;
    }

    public void Add(uint clilocId)
    {
        if (clilocId == 0) return;

        if (Header == 0)
        {
            Header = clilocId;
        }

        AddHash((int)clilocId);

        EnsureCapacity(6);
        var writer = new SpanWriter(_buffer.AsSpan(_bufferPos));
        writer.Write(clilocId);
        writer.Write((ushort)0); // No string arguments
        
        _bufferPos += 6;
    }

    public void Add(uint clilocId, string argument)
    {
        InternalAdd(clilocId, argument);
    }

    public void Add(string text)
    {
        var clilocId = StringNumbers[_stringNumbersIndex++ % StringNumbers.Length];
        InternalAdd(clilocId, text);
    }

    public void Add(uint clilocId, int value)
    {
        InternalAdd(clilocId, value.ToString(CultureInfo.InvariantCulture));
    }

    public void Add(uint clilocId, double value)
    {
        InternalAdd(clilocId, value.ToString(CultureInfo.InvariantCulture));
    }

    private void InternalAdd(uint clilocId, string? argument)
    {
        if (clilocId == 0) return;

        if (Header == 0)
        {
            Header = clilocId;
        }

        AddHash((int)clilocId);

        if (string.IsNullOrEmpty(argument))
        {
            EnsureCapacity(6);
            var w1 = new SpanWriter(_buffer.AsSpan(_bufferPos));
            w1.Write(clilocId);
            w1.Write((ushort)0);
            _bufferPos += 6;
            return;
        }

        AddHash(string.GetHashCode(argument, StringComparison.Ordinal));

        var textBytesLength = argument.Length * 2;
        EnsureCapacity(6 + textBytesLength);

        var writer = new SpanWriter(_buffer.AsSpan(_bufferPos));
        writer.Write(clilocId);
        writer.Write((ushort)textBytesLength);
        writer.Write(global::System.Text.Encoding.Unicode.GetBytes(argument));

        _bufferPos += writer.Position;
    }

    public override void Write(ref SpanWriter writer)
    {
        if (_buffer == null)
            throw new ObjectDisposedException(nameof(ObjectPropertyList));

        EnsureCapacity(4); // Terminator
        var termWriter = new SpanWriter(_buffer.AsSpan(_bufferPos));
        termWriter.Write((uint)0x00000000); // 4-byte terminator

        writer.Write(OpCode);
        writer.Write((ushort)(_bufferPos + 4 + 3)); // Calculate total length
        writer.Write(_buffer.AsSpan(0, _bufferPos + 4));
    }

    protected override bool ParsePayload(ref SpanReader reader) => false;

    public void Dispose()
    {
        if (_buffer != null)
        {
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = null;
        }
    }
}
