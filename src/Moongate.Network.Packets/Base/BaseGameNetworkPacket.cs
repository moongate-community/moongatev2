using Moongate.Network.Packets.Interfaces;
using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Base;

/// <summary>
/// Base implementation for game network packets with shared opcode and parsing validation logic.
/// </summary>
public abstract class BaseGameNetworkPacket : IGameNetworkPacket
{
    /// <summary>
    /// Gets the packet opcode identifier.
    /// </summary>
    public byte OpCode { get; }

    /// <summary>
    /// Gets the packet length. Use <c>-1</c> for variable-length packets.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Initializes a new packet base with opcode and expected length.
    /// </summary>
    /// <param name="opCode">Packet opcode.</param>
    /// <param name="length">Expected packet length, or <c>-1</c> for variable length.</param>
    protected BaseGameNetworkPacket(byte opCode, int length = -1)
    {
        OpCode = opCode;
        Length = length;
    }

    /// <summary>
    /// Tries to parse packet bytes after validating opcode and optional fixed length.
    /// </summary>
    /// <param name="data">Raw packet bytes.</param>
    /// <returns><c>true</c> when parsing succeeds; otherwise <c>false</c>.</returns>
    public bool TryParse(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
        {
            return false;
        }

        if (Length > -1 && data.Length != Length)
        {
            return false;
        }

        var reader = new SpanReader(data);

        try
        {
            if (reader.ReadByte() != OpCode)
            {
                return false;
            }

            return ParsePayload(ref reader);
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        finally
        {
            reader.Dispose();
        }
    }

    /// <summary>
    /// Writes packet content to the target span writer.
    /// </summary>
    /// <param name="writer">Destination writer.</param>
    public virtual void Write(ref SpanWriter writer)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Parses packet-specific payload after opcode validation.
    /// </summary>
    /// <param name="reader">Packet span reader positioned after opcode.</param>
    /// <returns><c>true</c> when payload parsing succeeds; otherwise <c>false</c>.</returns>
    protected abstract bool ParsePayload(ref SpanReader reader);
}
