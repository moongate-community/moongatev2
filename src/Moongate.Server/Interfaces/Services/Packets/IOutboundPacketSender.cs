using Moongate.Network.Client;
using Moongate.Server.Data.Packets;

namespace Moongate.Server.Interfaces.Services.Packets;

/// <summary>
/// Sends outbound packets to connected clients, handling serialization and logging.
/// </summary>
public interface IOutboundPacketSender
{
    /// <summary>
    /// Serializes and sends an outbound packet to a target client synchronously.
    /// Called from the game loop thread.
    /// </summary>
    /// <param name="client">Target connected client.</param>
    /// <param name="outgoingPacket">Outbound packet envelope.</param>
    /// <returns><c>true</c> when send succeeds; otherwise <c>false</c>.</returns>
    bool Send(MoongateTCPClient client, OutgoingGamePacket outgoingPacket);

    /// <summary>
    /// Serializes and sends an outbound packet to a target client asynchronously.
    /// </summary>
    /// <param name="client">Target connected client.</param>
    /// <param name="outgoingPacket">Outbound packet envelope.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> when send succeeds; otherwise <c>false</c>.</returns>
    Task<bool> SendAsync(
        MoongateTCPClient client,
        OutgoingGamePacket outgoingPacket,
        CancellationToken cancellationToken
    );
}
