using Moongate.Network.Packets.Interfaces;
using Moongate.Server.Data.Session;

namespace Moongate.Server.Interfaces.Listener;

/// <summary>
/// Handles parsed packets for a specific opcode.
/// </summary>
public interface IPacketListener
{
    /// <summary>
    /// Handles a parsed packet for the given session context.
    /// </summary>
    /// <param name="session">Source game session.</param>
    /// <param name="packet">Parsed packet instance.</param>
    /// <returns>
    /// <c>true</c> when the packet was handled successfully; otherwise <c>false</c>.
    /// </returns>
    Task<bool> HandlePacketAsync(GameSession session, IGameNetworkPacket packet);
}
