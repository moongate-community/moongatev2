using Moongate.Server.Data.Session;

namespace Moongate.Server.Data.Packets;

/// <summary>
/// Represents a parsed inbound packet with its source session.
/// </summary>
public readonly record struct GamePacket(
    ClientSession Session,
    byte PacketId,
    ReadOnlyMemory<byte> Data,
    long Timestamp
);
