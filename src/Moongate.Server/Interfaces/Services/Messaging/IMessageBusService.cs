using Moongate.Server.Data.Packets;

namespace Moongate.Server.Interfaces.Services.Messaging;

/// <summary>
/// Thread-safe in-process message bus for inbound packets crossing network and game-loop boundaries.
/// </summary>
public interface IMessageBusService
{
    /// <summary>
    /// Gets the current inbound packet queue depth.
    /// </summary>
    int CurrentQueueDepth { get; }

    /// <summary>
    /// Publishes an inbound packet to the game-loop side.
    /// </summary>
    /// <param name="packet">Inbound parsed packet.</param>
    void PublishIncomingPacket(IncomingGamePacket packet);

    /// <summary>
    /// Attempts to read the next inbound packet published by network threads.
    /// </summary>
    /// <param name="packet">Dequeued packet, if available.</param>
    /// <returns><c>true</c> when a packet is available; otherwise <c>false</c>.</returns>
    bool TryReadIncomingPacket(out IncomingGamePacket packet);
}
