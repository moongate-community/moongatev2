using Moongate.Network.Packets.Types.Packets;

namespace Moongate.Network.Packets.Data.Packets;

public readonly record struct PacketDescriptor(
    byte OpCode,
    PacketSizing Sizing,
    int Length,
    Type HandlerType
);
