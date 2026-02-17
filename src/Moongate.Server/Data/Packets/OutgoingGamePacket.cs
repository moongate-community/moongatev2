using Moongate.Network.Packets.Interfaces;

namespace Moongate.Server.Data.Packets;

/// <summary>
/// Represents an outbound packet queued for network send.
/// </summary>
public readonly record struct OutgoingGamePacket(
    long SessionId,
    IGameNetworkPacket Packet,
    long Timestamp
);
