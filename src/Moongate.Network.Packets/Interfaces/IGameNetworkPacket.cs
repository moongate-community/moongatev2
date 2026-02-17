using Moongate.Network.Spans;

namespace Moongate.Network.Packets.Interfaces;

/// <summary>
/// Defines the contract for parsing and writing a game network packet.
/// </summary>
public interface IGameNetworkPacket
{
    /// <summary>
    /// Gets the packet opcode identifier.
    /// </summary>
    byte OpCode { get; }

    /// <summary>
    /// Gets the packet length. Use <c>-1</c> for variable-length packets.
    /// </summary>
    int Length { get; }

    /// <summary>
    /// Attempts to parse packet data from the given input span.
    /// </summary>
    /// <param name="data">Raw packet bytes.</param>
    /// <returns><c>true</c> when parsing succeeds; otherwise <c>false</c>.</returns>
    bool TryParse(ReadOnlySpan<byte> data);

    /// <summary>
    /// Writes packet bytes to the target span writer.
    /// </summary>
    /// <param name="writer">Destination writer.</param>
    void Write(ref SpanWriter writer);
}
