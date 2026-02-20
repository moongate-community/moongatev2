using DryIoc;
using Moongate.Abstractions.Extensions;
using Moongate.Scripting.Interfaces;
using Moongate.Scripting.Services;
using Moongate.Server.Interfaces.Services.Console;
using Moongate.Server.Interfaces.Services.Events;
using Moongate.Server.Interfaces.Services.Files;
using Moongate.Server.Interfaces.Services.GameLoop;
using Moongate.Server.Interfaces.Services.Metrics;
using Moongate.Server.Interfaces.Services.Network;
using Moongate.Server.Interfaces.Services.Persistence;
using Moongate.Server.Services.Console;
using Moongate.Server.Services.Events;
using Moongate.Server.Services.Files;
using Moongate.Server.Services.GameLoop;
using Moongate.Server.Services.Metrics;
using Moongate.Server.Services.Network;
using Moongate.Server.Services.Persistence;

namespace Moongate.Server.Extensions.Bootstrap;

/// <summary>
/// Registers host-managed Moongate services with startup priorities.
/// </summary>
public static class AddBootstrapHostedServicesExtension
{
    /// <summary>
    /// Registers host lifecycle services used during bootstrap run loop.
    /// </summary>
    public static Container AddBootstrapHostedServices(this Container container)
    {
        container.RegisterMoongateService<IPersistenceService, PersistenceService>(110);
        container.RegisterMoongateService<IFileLoaderService, FileLoaderService>(120);
        container.RegisterMoongateService<IGameLoopService, GameLoopService>(130);
        container.RegisterMoongateService<ICommandSystemService, CommandSystemService>(131);
        container.RegisterMoongateService<IConsoleCommandService, ConsoleCommandService>(132);
        container.RegisterMoongateService<IMetricsCollectionService, MetricsCollectionService>(135);
        container.RegisterMoongateService<IGameEventScriptBridgeService, GameEventScriptBridgeService>(140);
        container.RegisterMoongateService<INetworkService, NetworkService>(150);
        container.RegisterMoongateService<IScriptEngineService, LuaScriptEngineService>(150);

        return container;
    }
}
