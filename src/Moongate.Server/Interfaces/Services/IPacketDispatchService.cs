using Moongate.Server.Data.Packets;
using Moongate.Server.Interfaces.Listener;

namespace Moongate.Server.Interfaces.Services;

/// <summary>
/// Stores packet listeners and dispatches parsed packets to them.
/// </summary>
public interface IPacketDispatchService
{
    /// <summary>
    /// Registers a listener for an opcode.
    /// </summary>
    /// <param name="opCode">Packet opcode.</param>
    /// <param name="packetListener">Listener instance.</param>
    void AddPacketListener(byte opCode, IPacketListener packetListener);

    /// <summary>
    /// Dispatches a packet to all listeners registered for an opcode.
    /// </summary>
    /// <param name="gamePacket">Parsed packet with session context.</param>
    /// <returns><c>true</c> when at least one listener was notified.</returns>
    bool NotifyPacketListeners(IncomingGamePacket gamePacket);
}
