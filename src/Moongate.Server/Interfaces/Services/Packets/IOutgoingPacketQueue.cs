using Moongate.Network.Packets.Interfaces;
using Moongate.Server.Data.Packets;

namespace Moongate.Server.Interfaces.Services.Packets;

/// <summary>
/// Queue contract for outbound packets produced during game-loop processing.
/// </summary>
public interface IOutgoingPacketQueue
{
    /// <summary>
    /// Enqueues an outbound packet for a target session.
    /// </summary>
    /// <param name="sessionId">Target session identifier.</param>
    /// <param name="packet">Packet to send.</param>
    void Enqueue(long sessionId, IGameNetworkPacket packet);

    /// <summary>
    /// Attempts to dequeue the next outbound packet.
    /// </summary>
    /// <param name="gamePacket">Dequeued outbound packet.</param>
    /// <returns><c>true</c> if an item was dequeued; otherwise <c>false</c>.</returns>
    bool TryDequeue(out OutgoingGamePacket gamePacket);
}
