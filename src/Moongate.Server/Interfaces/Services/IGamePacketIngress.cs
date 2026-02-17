using Moongate.Server.Data.Packets;

namespace Moongate.Server.Interfaces.Services;

/// <summary>
/// Contract for accepting parsed inbound game packets.
/// </summary>
public interface IGamePacketIngress
{
    /// <summary>
    /// Enqueues an inbound packet for game-loop processing.
    /// </summary>
    /// <param name="gamePacket">Parsed packet metadata and payload.</param>
    void EnqueueGamePacket(GamePacket gamePacket);
}
