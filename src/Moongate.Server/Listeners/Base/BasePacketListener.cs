using Moongate.Network.Packets.Interfaces;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Listener;
using Moongate.Server.Interfaces.Services;

namespace Moongate.Server.Listeners.Base;

/// <summary>
/// Base packet-listener implementation with outbound queue helpers.
/// </summary>
public abstract class BasePacketListener : IPacketListener
{
    private readonly IOutgoingPacketQueue _outgoingPacketQueue;

    protected BasePacketListener(IOutgoingPacketQueue outgoingPacketQueue)
        => _outgoingPacketQueue = outgoingPacketQueue;

    /// <inheritdoc />
    public Task<bool> HandlePacketAsync(GameNetworkSession session, IGameNetworkPacket packet)
        => HandleCoreAsync(session, packet);

    /// <summary>
    /// Enqueues an outbound packet for the given session.
    /// </summary>
    /// <param name="session">Target session.</param>
    /// <param name="packet">Packet to send.</param>
    protected void Enqueue(GameNetworkSession session, IGameNetworkPacket packet)
        => _outgoingPacketQueue.Enqueue(session.SessionId, packet);

    /// <summary>
    /// Enqueues an outbound packet for the specified session id.
    /// </summary>
    /// <param name="sessionId">Target session id.</param>
    /// <param name="packet">Packet to send.</param>
    protected void Enqueue(long sessionId, IGameNetworkPacket packet)
        => _outgoingPacketQueue.Enqueue(sessionId, packet);

    /// <summary>
    /// Handles the parsed inbound packet.
    /// </summary>
    /// <param name="session">Source session.</param>
    /// <param name="packet">Parsed packet instance.</param>
    /// <returns><c>true</c> when handled; otherwise <c>false</c>.</returns>
    protected abstract Task<bool> HandleCoreAsync(GameNetworkSession session, IGameNetworkPacket packet);
}
