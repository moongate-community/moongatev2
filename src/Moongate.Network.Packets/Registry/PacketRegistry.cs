using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Data.Internal.Packets;
using Moongate.Network.Packets.Data.Packets;
using Moongate.Network.Packets.Interfaces;
using Moongate.Network.Packets.Types.Packets;

namespace Moongate.Network.Packets.Registry;

public class PacketRegistry
{
    private readonly Dictionary<byte, PacketRegistration> _registrations = [];

    public IReadOnlyList<PacketDescriptor> RegisteredPackets
        =>
        [
            .. _registrations.Values
                             .Select(static registration => registration.Descriptor)
                             .OrderBy(static descriptor => descriptor.OpCode)
        ];

    public void RegisterFixed<TPacket>(byte opcode, int length, string? description = null)
        where TPacket : IGameNetworkPacket, new()
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Fixed packet length must be greater than zero.");
        }

        Register<TPacket>(opcode, PacketSizing.Fixed, length, description);
    }

    public void RegisterFromAttribute<TPacket>()
        where TPacket : IGameNetworkPacket, new()
    {
        var attribute = typeof(TPacket).GetCustomAttributes(typeof(PacketHandlerAttribute), false)
                                       .OfType<PacketHandlerAttribute>()
                                       .SingleOrDefault();

        if (attribute is null)
        {
            throw new InvalidOperationException($"Packet type '{typeof(TPacket).Name}' is missing PacketHandlerAttribute.");
        }

        if (attribute.Sizing == PacketSizing.Fixed)
        {
            RegisterFixed<TPacket>(attribute.OpCode, attribute.Length, attribute.Description);

            return;
        }

        RegisterVariable<TPacket>(attribute.OpCode, attribute.Description);
    }

    public void RegisterVariable<TPacket>(byte opcode, string? description = null)
        where TPacket : IGameNetworkPacket, new()
        => Register<TPacket>(opcode, PacketSizing.Variable, -1, description);

    public bool TryCreatePacket(byte opcode, out IGameNetworkPacket? packet)
    {
        if (_registrations.TryGetValue(opcode, out var registration))
        {
            packet = registration.Factory();

            return true;
        }

        packet = null;

        return false;
    }

    public bool TryGetDescriptor(byte opcode, out PacketDescriptor descriptor)
    {
        if (_registrations.TryGetValue(opcode, out var registration))
        {
            descriptor = registration.Descriptor;

            return true;
        }

        descriptor = default;

        return false;
    }

    private void Register<TPacket>(byte opcode, PacketSizing sizing, int length, string? description)
        where TPacket : IGameNetworkPacket, new()
    {
        if (_registrations.ContainsKey(opcode))
        {
            throw new InvalidOperationException($"Packet opcode 0x{opcode:X2} is already registered.");
        }

        var resolvedDescription = ResolveDescription(opcode, typeof(TPacket), description);
        var descriptor = new PacketDescriptor(opcode, sizing, length, resolvedDescription, typeof(TPacket));
        var registration = new PacketRegistration(descriptor, static () => new TPacket());
        _registrations.Add(opcode, registration);
    }

    private static string ResolveDescription(byte opcode, Type packetType, string? explicitDescription)
    {
        _ = opcode;

        if (!string.IsNullOrWhiteSpace(explicitDescription))
        {
            return explicitDescription;
        }

        return packetType.Name.Replace("Packet", string.Empty, StringComparison.Ordinal);
    }
}
