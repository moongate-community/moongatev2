using Moongate.Network.Packets.Data.Packets;
using Moongate.Network.Packets.Interfaces;

namespace Moongate.Network.Packets.Data.Internal.Packets;

internal readonly record struct PacketRegistration(
    PacketDescriptor Descriptor,
    Func<IGameNetworkPacket> Factory
);
