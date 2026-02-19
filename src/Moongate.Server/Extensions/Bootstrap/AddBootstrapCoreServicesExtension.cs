using DryIoc;
using Moongate.Server.Interfaces.Services.Accounting;
using Moongate.Server.Interfaces.Services.Events;
using Moongate.Server.Interfaces.Services.Files;
using Moongate.Server.Interfaces.Services.Lifecycle;
using Moongate.Server.Interfaces.Services.Messaging;
using Moongate.Server.Interfaces.Services.Packets;
using Moongate.Server.Interfaces.Services.Sessions;
using Moongate.Server.Interfaces.Services.Timing;
using Moongate.Server.Services.Accounting;
using Moongate.Server.Services.Events;
using Moongate.Server.Services.Files;
using Moongate.Server.Services.Lifecycle;
using Moongate.Server.Services.Messaging;
using Moongate.Server.Services.Packets;
using Moongate.Server.Services.Sessions;
using Moongate.Server.Services.Timing;

namespace Moongate.Server.Extensions.Bootstrap;

/// <summary>
/// Registers core non-hosted services required by server runtime.
/// </summary>
public static class AddBootstrapCoreServicesExtension
{
    /// <summary>
    /// Registers base messaging, dispatch, session and utility services.
    /// </summary>
    public static Container AddBootstrapCoreServices(this Container container)
    {
        container.Register<IMessageBusService, MessageBusService>(Reuse.Singleton);
        container.Register<IGameEventBusService, GameEventBusService>(Reuse.Singleton);
        container.Register<IServerLifetimeService, ServerLifetimeService>(Reuse.Singleton);
        container.Register<IOutgoingPacketQueue, OutgoingPacketQueue>(Reuse.Singleton);
        container.Register<IOutboundPacketSender, OutboundPacketSender>(Reuse.Singleton);
        container.Register<IPacketDispatchService, PacketDispatchService>(Reuse.Singleton);
        container.Register<IGameNetworkSessionService, GameNetworkSessionService>(Reuse.Singleton);
        container.Register<ITimerService, TimerWheelService>(Reuse.Singleton);
        container.Register<IAccountService, AccountService>(Reuse.Singleton);

        return container;
    }
}
