using DryIoc;
using Moongate.Network.Packets.Data.Packets;
using Moongate.Server.Handlers;
using Moongate.Server.Interfaces.Listener;
using Moongate.Server.Interfaces.Services.Packets;

namespace Moongate.Server.Bootstrap.Internal;

/// <summary>
/// Registers packet listeners for built-in packet opcodes.
/// </summary>
internal static class BootstrapPacketHandlerRegistration
{
    public static void Register(Container container)
    {
        RegisterPacketHandler<LoginHandler>(container, PacketDefinition.LoginSeedPacket);
        RegisterPacketHandler<LoginHandler>(container, PacketDefinition.AccountLoginPacket);
        RegisterPacketHandler<LoginHandler>(container, PacketDefinition.ServerSelectPacket);
        RegisterPacketHandler<LoginHandler>(container, PacketDefinition.GameLoginPacket);
        RegisterPacketHandler<LoginHandler>(container, PacketDefinition.LoginCharacterPacket);

        RegisterPacketHandler<CharacterHandler>(container, PacketDefinition.CharacterCreationPacket);

        RegisterPacketHandler<PingPongHandler>(container, PacketDefinition.PingMessagePacket);
        RegisterPacketHandler<PlayerStatusHandler>(container, PacketDefinition.GetPlayerStatusPacket);
        RegisterPacketHandler<MovementHandler>(container, PacketDefinition.MoveRequestPacket);
    }

    private static void RegisterPacketHandler<T>(Container container, byte opCode) where T : IPacketListener
    {
        if (!container.IsRegistered<T>())
        {
            container.Register<T>();
        }

        var handler = container.Resolve<T>();
        var packetListenerService = container.Resolve<IPacketDispatchService>();
        packetListenerService.AddPacketListener(opCode, handler);
    }
}
