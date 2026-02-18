using Moongate.Network.Client;
using Moongate.Server.Data.Packets;

namespace Moongate.Server.Interfaces.Services;

/// <summary>
/// Sends outbound packets to connected clients, handling serialization and logging.
/// </summary>
public interface IOutboundPacketSender
{
    /// <summary>
    /// Serializes and sends an outbound packet to a target client.
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
