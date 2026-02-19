using DryIoc;
using Moongate.Abstractions.Extensions;
using Moongate.Core.Data.Directories;
using Moongate.Server.Data.Config;
using Moongate.Server.Interfaces.Services.Accounting;
using Moongate.Server.Interfaces.Services.Console;
using Moongate.Server.Interfaces.Services.Events;
using Moongate.Server.Interfaces.Services.Files;
using Moongate.Server.Interfaces.Services.GameLoop;
using Moongate.Server.Interfaces.Services.Lifecycle;
using Moongate.Server.Interfaces.Services.Messaging;
using Moongate.Server.Interfaces.Services.Metrics;
using Moongate.Server.Interfaces.Services.Network;
using Moongate.Server.Interfaces.Services.Packets;
using Moongate.Server.Interfaces.Services.Persistence;
using Moongate.Server.Interfaces.Services.Sessions;
using Moongate.Server.Interfaces.Services.Timing;
using Moongate.Server.Services.Accounting;
using Moongate.Server.Services.Console;
using Moongate.Server.Services.Events;
using Moongate.Server.Services.Files;
using Moongate.Server.Services.GameLoop;
using Moongate.Server.Services.Lifecycle;
using Moongate.Server.Services.Messaging;
using Moongate.Server.Services.Metrics;
using Moongate.Server.Services.Metrics.Providers;
using Moongate.Server.Services.Network;
using Moongate.Server.Services.Packets;
using Moongate.Server.Services.Persistence;
using Moongate.Server.Services.Sessions;
using Moongate.Server.Services.Timing;
using Moongate.Scripting.Interfaces;
using Moongate.Scripting.Services;

namespace Moongate.Server.Bootstrap.Internal;

/// <summary>
/// Registers core server services and their dependencies in the DI container.
/// </summary>
internal static class BootstrapServiceRegistration
{
    public static void Register(
        Container container,
        MoongateConfig config,
        DirectoriesConfig directoriesConfig,
        IConsoleUiService consoleUiService
    )
    {
        var timerServiceConfig = new TimerServiceConfig
        {
            TickDuration = TimeSpan.FromMilliseconds(Math.Max(1, config.Game.TimerTickMilliseconds)),
            WheelSize = Math.Max(1, config.Game.TimerWheelSize)
        };

        container.RegisterInstance(config);
        container.RegisterInstance(config.Metrics);
        container.RegisterInstance(directoriesConfig);
        container.RegisterInstance(timerServiceConfig);
        container.RegisterInstance(consoleUiService);

        container.Register<IMessageBusService, MessageBusService>(Reuse.Singleton);
        container.Register<IGameEventBusService, GameEventBusService>(Reuse.Singleton);
        container.Register<IServerLifetimeService, ServerLifetimeService>(Reuse.Singleton);
        container.Register<IOutgoingPacketQueue, OutgoingPacketQueue>(Reuse.Singleton);
        container.Register<IOutboundPacketSender, OutboundPacketSender>(Reuse.Singleton);
        container.Register<IPacketDispatchService, PacketDispatchService>(Reuse.Singleton);
        container.Register<IGameNetworkSessionService, GameNetworkSessionService>(Reuse.Singleton);
        container.Register<ITimerService, TimerWheelService>(Reuse.Singleton);

        container.RegisterDelegate<IGameLoopMetricsSource>(
            resolver => (IGameLoopMetricsSource)resolver.Resolve<IGameLoopService>(),
            Reuse.Singleton
        );
        container.RegisterDelegate<INetworkMetricsSource>(
            resolver => (INetworkMetricsSource)resolver.Resolve<INetworkService>(),
            Reuse.Singleton
        );
        container.RegisterDelegate<IPersistenceMetricsSource>(
            resolver => (IPersistenceMetricsSource)resolver.Resolve<IPersistenceService>(),
            Reuse.Singleton
        );
        container.Register<IMetricsHttpSnapshotFactory, MetricsHttpSnapshotFactory>(Reuse.Singleton);
        container.Register<IMetricProvider, GameLoopMetricsProvider>(Reuse.Singleton);
        container.Register<IMetricProvider, NetworkMetricsProvider>(Reuse.Singleton);
        container.Register<IMetricProvider, ScriptEngineMetricsProvider>(Reuse.Singleton);
        container.Register<IMetricProvider, PersistenceMetricsProvider>(Reuse.Singleton);

        container.Register<IAccountService, AccountService>(Reuse.Singleton);

        container.RegisterMoongateService<IPersistenceService, PersistenceService>(110);
        container.RegisterMoongateService<IGameLoopService, GameLoopService>(130);
        container.RegisterMoongateService<ICommandSystemService, CommandSystemService>(131);
        container.RegisterMoongateService<IConsoleCommandService, ConsoleCommandService>(132);
        container.RegisterMoongateService<IMetricsCollectionService, MetricsCollectionService>(135);
        container.RegisterMoongateService<INetworkService, NetworkService>(150);
        container.RegisterMoongateService<IFileLoaderService, FileLoaderService>(120);
        container.RegisterMoongateService<IGameEventScriptBridgeService, GameEventScriptBridgeService>(140);
        container.RegisterMoongateService<IScriptEngineService, LuaScriptEngineService>(150);
    }
}
