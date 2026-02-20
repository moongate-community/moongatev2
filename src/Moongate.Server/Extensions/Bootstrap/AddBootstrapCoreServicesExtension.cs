using DryIoc;
using Moongate.Server.Interfaces.Characters;
using Moongate.Server.Interfaces.Services.Accounting;
using Moongate.Server.Interfaces.Services.Entities;
using Moongate.Server.Interfaces.Services.Events;
using Moongate.Server.Interfaces.Services.Lifecycle;
using Moongate.Server.Interfaces.Services.Messaging;
using Moongate.Server.Interfaces.Services.Packets;
using Moongate.Server.Interfaces.Services.Sessions;
using Moongate.Server.Interfaces.Services.Timing;
using Moongate.Server.Services.Accounting;
using Moongate.Server.Services.Characters;
using Moongate.Server.Services.Entities;
using Moongate.Server.Services.Events;
using Moongate.Server.Services.Lifecycle;
using Moongate.Server.Services.Messaging;
using Moongate.Server.Services.Packets;
using Moongate.Server.Services.Sessions;
using Moongate.Server.Services.Timing;
using Moongate.UO.Data.Interfaces.Names;
using Moongate.UO.Data.Interfaces.Templates;
using Moongate.UO.Data.Services.Names;
using Moongate.UO.Data.Services.Templates;

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
        container.Register<IEntityFactoryService, EntityFactoryService>(Reuse.Singleton);
        container.Register<IOutgoingPacketQueue, OutgoingPacketQueue>(Reuse.Singleton);
        container.Register<IOutboundPacketSender, OutboundPacketSender>(Reuse.Singleton);
        container.Register<IPacketDispatchService, PacketDispatchService>(Reuse.Singleton);
        container.Register<IGameNetworkSessionService, GameNetworkSessionService>(Reuse.Singleton);
        container.Register<ITimerService, TimerWheelService>(Reuse.Singleton);
        container.Register<IAccountService, AccountService>(Reuse.Singleton);
        container.Register<ICharacterService, CharacterService>(Reuse.Singleton);
        container.Register<INameService, NameService>(Reuse.Singleton);
        container.Register<IItemTemplateService, ItemTemplateService>(Reuse.Singleton);
        container.Register<IMobileTemplateService, MobileTemplateService>(Reuse.Singleton);
        container.Register<IStartupTemplateService, StartupTemplateService>(Reuse.Singleton);

        return container;
    }
}
