using Moongate.Abstractions.Interfaces.Services.Base;
using Moongate.Server.Data.Packets;
using Moongate.Server.Interfaces.Listener;

namespace Moongate.Server.Interfaces.Services;

/// <summary>
/// Defines the network service contract for receiving and exposing parsed packets.
/// </summary>
public interface INetworkService : IMoongateService, IDisposable
{
    /// <summary>
    /// Gets a snapshot of parsed packets currently queued by the network layer.
    /// </summary>
    IReadOnlyCollection<IncomingGamePacket> ParsedPackets { get; }

    /// <summary>
    /// Registers a listener for the specified packet opcode.
    /// </summary>
    /// <param name="OpCode">Packet opcode.</param>
    /// <param name="packetListener">Listener to register.</param>
    void AddPacketListener(byte OpCode, IPacketListener packetListener);

    /// <summary>
    /// Attempts to dequeue the next parsed packet.
    /// </summary>
    /// <param name="gamePacket">Dequeued packet if available.</param>
    /// <returns><c>true</c> when a packet is dequeued; otherwise <c>false</c>.</returns>
    bool TryDequeueParsedPacket(out IncomingGamePacket gamePacket);
}
