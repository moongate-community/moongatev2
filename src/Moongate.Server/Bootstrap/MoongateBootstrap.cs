using System.Diagnostics;
using DryIoc;
using Moongate.Abstractions.Data.Internal;
using Moongate.Abstractions.Extensions;
using Moongate.Abstractions.Interfaces.Services.Base;
using Moongate.Core.Data.Directories;
using Moongate.Core.Extensions.Directories;
using Moongate.Core.Extensions.Logger;
using Moongate.Core.Json;
using Moongate.Core.Types;
using Moongate.Network.Packets.Data.Packets;
using Moongate.Scripting.Data.Config;
using Moongate.Scripting.Extensions.Scripts;
using Moongate.Scripting.Interfaces;
using Moongate.Scripting.Modules;
using Moongate.Scripting.Services;
using Moongate.Server.Data.Config;
using Moongate.Server.Data.Events;
using Moongate.Server.FileLoaders;
using Moongate.Server.Handlers;
using Moongate.Server.Http;
using Moongate.Server.Http.Interfaces;
using Moongate.Server.Interfaces.Listener;
using Moongate.Server.Interfaces.Services.Console;
using Moongate.Server.Interfaces.Services.Events;
using Moongate.Server.Interfaces.Services.Files;
using Moongate.Server.Interfaces.Services.Lifecycle;
using Moongate.Server.Interfaces.Services.GameLoop;
using Moongate.Server.Interfaces.Services.Messaging;
using Moongate.Server.Interfaces.Services.Metrics;
using Moongate.Server.Interfaces.Services.Network;
using Moongate.Server.Interfaces.Services.Packets;
using Moongate.Server.Interfaces.Services.Persistence;
using Moongate.Server.Interfaces.Services.Sessions;
using Moongate.Server.Interfaces.Services.Timing;
using Moongate.Server.Json;
using Moongate.Server.Services.Console;
using Moongate.Server.Services.Console.Internal.Logging;
using Moongate.Server.Services.Events;
using Moongate.Server.Services.Files;
using Moongate.Server.Services.Lifecycle;
using Moongate.Server.Services.GameLoop;
using Moongate.Server.Services.Messaging;
using Moongate.Server.Services.Metrics;
using Moongate.Server.Services.Metrics.Providers;
using Moongate.Server.Services.Network;
using Moongate.Server.Services.Packets;
using Moongate.Server.Services.Persistence;
using Moongate.Server.Services.Sessions;
using Moongate.Server.Services.Timing;
using Moongate.UO.Data.Files;
using Moongate.UO.Data.Version;
using Serilog;
using Serilog.Filters;

namespace Moongate.Server.Bootstrap;

public sealed class MoongateBootstrap : IDisposable
{
    private readonly Container _container = new(Rules.Default.WithUseInterpretation());

    private ILogger _logger;

    private DirectoriesConfig _directoriesConfig;
    private readonly IConsoleUiService _consoleUiService = new ConsoleUiService();
    private readonly MoongateConfig _moongateConfig;

    public MoongateBootstrap(MoongateConfig config)
    {
        _moongateConfig = config;

        CheckDirectoryConfig();

        CreateLogger();
        CheckConfig();
        CheckUODirectory();
        EnsureDataAssets();
        Console.WriteLine("Root Directory: " + _directoriesConfig.Root);

        RegisterHttpServer();
        RegisterScriptUserData();
        RegisterScriptModules();
        RegisterServices();
        RegisterFileLoaders();

        RegisterPacketHandlers();
    }

    public void Dispose()
    {
        _container.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var startTime = Stopwatch.GetTimestamp();
        var serviceRegistrations = _container.Resolve<List<ServiceRegistrationObject>>()
                                             .OrderBy(s => s.Priority)
                                             .ToList();

        var runningServices = new List<IMoongateService>(serviceRegistrations.Count);

        foreach (var serviceRegistration in serviceRegistrations)
        {
            if (_container.Resolve(serviceRegistration.ServiceType) is not IMoongateService instance)
            {
                throw new InvalidOperationException(
                    $"Failed to resolve service of type {serviceRegistration.ServiceType.FullName}"
                );
            }

            _logger.Verbose("Starting {ServiceTypeFullName}", serviceRegistration.ImplementationType.Name);
            await instance.StartAsync();
            runningServices.Add(instance);
        }

        _logger.Information("Server started in {StartupTime} ms", Stopwatch.GetElapsedTime(startTime).TotalMilliseconds);
        _logger.Information("Moongate server is running. Press Ctrl+C to stop.");

        var serverLifetimeService = _container.Resolve<IServerLifetimeService>();
        using var linkedCancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, serverLifetimeService.ShutdownToken);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, linkedCancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.Information("Shutdown requested.");
        }

        await StopAsync(runningServices);
    }

    private void CheckConfig()
    {
        if (!File.Exists(Path.Combine(_directoriesConfig.Root, "moongate.json")))
        {
            _logger.Warning(
                "No moongate.json configuration file found in root directory. Using default configuration values."
            );

            JsonUtils.SerializeToFile(
                _moongateConfig,
                Path.Combine(_directoriesConfig.Root, "moongate.json"),
                MoongateServerJsonContext.Default
            );
        }

        else
        {
            var fileConfig = JsonUtils.DeserializeFromFile<MoongateConfig>(
                Path.Combine(_directoriesConfig.Root, "moongate.json"),
                MoongateServerJsonContext.Default
            );

            _logger.Information("Loaded configuration from moongate.json in root directory.");

            // Override properties with values from the file if they are not null or default
            if (!string.IsNullOrWhiteSpace(fileConfig.RootDirectory))
            {
                _moongateConfig.RootDirectory = fileConfig.RootDirectory;
            }

            if (!string.IsNullOrWhiteSpace(fileConfig.UODirectory))
            {
                _moongateConfig.UODirectory = fileConfig.UODirectory;
            }

            if (fileConfig.LogLevel != LogLevelType.Information)
            {
                _moongateConfig.LogLevel = fileConfig.LogLevel;
            }

            _moongateConfig.LogPacketData = fileConfig.LogPacketData;
        }
    }

    private void CheckDirectoryConfig()
    {
        if (string.IsNullOrWhiteSpace(_moongateConfig.RootDirectory))
        {
            _moongateConfig.RootDirectory = Environment.GetEnvironmentVariable("MOONGATE_ROOT_DIRECTORY") ??
                                            Path.Combine(AppContext.BaseDirectory, "moongate");
        }

        _moongateConfig.RootDirectory = _moongateConfig.RootDirectory.ResolvePathAndEnvs();

        _directoriesConfig = new(_moongateConfig.RootDirectory, Enum.GetNames<DirectoryType>());
    }

    private void CheckUODirectory()
    {
        if (string.IsNullOrWhiteSpace(_moongateConfig.UODirectory))
        {
            _moongateConfig.UODirectory = Environment.GetEnvironmentVariable("MOONGATE_UO_DIRECTORY");
        }

        if (string.IsNullOrWhiteSpace(_moongateConfig.UODirectory))
        {
            _logger.Error("UO Directory not configured. Set --uoDirectory or MOONGATE_UO_DIRECTORY.");

            throw new InvalidOperationException("UO Directory not configured.");
        }

        UoFiles.RootDir = _moongateConfig.UODirectory.ResolvePathAndEnvs();
        UoFiles.ReLoadDirectory();
        _logger.Information("UO Directory configured in {UODirectory}", UoFiles.RootDir);
    }

    private void CreateLogger()
    {
        var appLogPath = Path.Combine(_directoriesConfig[DirectoryType.Logs], "moongate-.log");
        var packetLogPath = Path.Combine(_directoriesConfig[DirectoryType.Logs], "packets-.log");
        var configuration = new LoggerConfiguration()
                            .MinimumLevel
                            .Is(_moongateConfig.LogLevel.ToSerilogLogLevel())
                            .WriteTo
                            .File(
                                appLogPath,
                                rollingInterval: RollingInterval.Day
                            );

        if (_moongateConfig.Metrics.LogToConsole)
        {
            configuration = configuration.WriteTo.Sink(new ConsoleUiSerilogSink(_consoleUiService));
        }
        else
        {
            configuration = configuration.WriteTo.Logger(
                loggerConfiguration =>
                    loggerConfiguration
                        .Filter
                        .ByExcluding(Matching.WithProperty("MetricsData"))
                        .WriteTo
                        .Sink(new ConsoleUiSerilogSink(_consoleUiService))
            );
        }

        if (_moongateConfig.LogPacketData)
        {
            configuration = configuration.WriteTo.Logger(
                loggerConfiguration =>
                    loggerConfiguration
                        .Filter
                        .ByIncludingOnly(Matching.WithProperty("PacketData"))
                        .WriteTo
                        .File(
                            packetLogPath,
                            rollingInterval: RollingInterval.Day,
                            outputTemplate:
                            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
                        )
            );
        }

        Log.Logger = configuration.CreateLogger();
        _logger = Log.ForContext<MoongateBootstrap>();
    }

    private void EnsureDataAssets()
    {
        var sourceDataDirectory = Path.Combine(AppContext.BaseDirectory, "Assets", "data");
        var destinationDataDirectory = _directoriesConfig[DirectoryType.Data];

        DataAssetsBootstrapper.EnsureDataAssets(sourceDataDirectory, destinationDataDirectory, _logger);
    }

    private void RegisterFileLoaders()
    {
        var fileLoaderService = _container.Resolve<IFileLoaderService>();

        fileLoaderService.AddFileLoader<ClientVersionLoader>();
        fileLoaderService.AddFileLoader<SkillLoader>();
        fileLoaderService.AddFileLoader<ExpansionLoader>();
        fileLoaderService.AddFileLoader<BodyDataLoader>();
        fileLoaderService.AddFileLoader<ProfessionsLoader>();
        fileLoaderService.AddFileLoader<MultiDataLoader>();
        fileLoaderService.AddFileLoader<RaceLoader>();
        fileLoaderService.AddFileLoader<TileDataLoader>();
        fileLoaderService.AddFileLoader<MapLoader>();
        fileLoaderService.AddFileLoader<CliLocLoader>();
        fileLoaderService.AddFileLoader<ContainersDataLoader>();
        fileLoaderService.AddFileLoader<RegionDataLoader>();
        fileLoaderService.AddFileLoader<WeatherDataLoader>();
        fileLoaderService.AddFileLoader<NamesLoader>();
    }

    private void RegisterHttpServer()
    {
        if (_moongateConfig.Http.IsEnabled)
        {
            _container.RegisterMoongateService<IMoongateHttpService, MoongateHttpService>(200);
            _logger.Information("HTTP Server enabled.");

            var httpServiceOptions = new MoongateHttpServiceOptions
            {
                DirectoriesConfig = _directoriesConfig,
                IsOpenApiEnabled = _moongateConfig.Http.IsOpenApiEnabled,
                Port = _moongateConfig.Http.Port,
                ServiceMappings = null,
                MinimumLogLevel = _moongateConfig.LogLevel.ToSerilogLogLevel()
            };

            _container.RegisterInstance(httpServiceOptions);
        }
        else
        {
            _logger.Information("HTTP Server disabled.");
        }
    }

    private void RegisterPacketHandler<T>(byte opCode) where T : IPacketListener
    {
        if (!_container.IsRegistered<T>())
        {
            _container.Register<T>();
        }

        var handler = _container.Resolve<T>();
        var packetListenerService = _container.Resolve<IPacketDispatchService>();
        packetListenerService.AddPacketListener(opCode, handler);
    }

    private void RegisterPacketHandlers()
    {
        RegisterPacketHandler<LoginHandler>(PacketDefinition.LoginSeedPacket);
        RegisterPacketHandler<LoginHandler>(PacketDefinition.AccountLoginPacket);
        RegisterPacketHandler<LoginHandler>(PacketDefinition.ServerSelectPacket);
        RegisterPacketHandler<LoginHandler>(PacketDefinition.GameLoginPacket);
    }

    private void RegisterScriptModules()
    {
        _container.RegisterInstance(
            new LuaEngineConfig(
                _directoriesConfig[DirectoryType.Scripts],
                _directoriesConfig[DirectoryType.Scripts],
                "0.1.0"
            )
        );
        _container.RegisterScriptModule<LogModule>();
    }

    private void RegisterScriptUserData()
    {
        _container.RegisterLuaUserData<PlayerConnectedEvent>();
        _container.RegisterLuaUserData<PlayerDisconnectedEvent>();
        _container.RegisterLuaUserData<ClientVersion>();
    }

    private void RegisterServices()
    {
        var timerServiceConfig = new TimerServiceConfig
        {
            TickDuration = TimeSpan.FromMilliseconds(Math.Max(1, _moongateConfig.Game.TimerTickMilliseconds)),
            WheelSize = Math.Max(1, _moongateConfig.Game.TimerWheelSize)
        };

        _container.RegisterInstance(_moongateConfig);
        _container.RegisterInstance(_moongateConfig.Metrics);
        _container.RegisterInstance(_directoriesConfig);
        _container.RegisterInstance(timerServiceConfig);
        _container.RegisterInstance(_consoleUiService);

        _container.Register<IMessageBusService, MessageBusService>(Reuse.Singleton);
        _container.Register<IGameEventBusService, GameEventBusService>(Reuse.Singleton);
        _container.Register<IServerLifetimeService, ServerLifetimeService>(Reuse.Singleton);
        _container.Register<IOutgoingPacketQueue, OutgoingPacketQueue>(Reuse.Singleton);
        _container.Register<IOutboundPacketSender, OutboundPacketSender>(Reuse.Singleton);
        _container.Register<IPacketDispatchService, PacketDispatchService>(Reuse.Singleton);
        _container.Register<IGameNetworkSessionService, GameNetworkSessionService>(Reuse.Singleton);
        _container.Register<ITimerService, TimerWheelService>(Reuse.Singleton);

        _container.RegisterDelegate<IGameLoopMetricsSource>(
            resolver => (IGameLoopMetricsSource)resolver.Resolve<IGameLoopService>(),
            Reuse.Singleton
        );
        _container.RegisterDelegate<INetworkMetricsSource>(
            resolver => (INetworkMetricsSource)resolver.Resolve<INetworkService>(),
            Reuse.Singleton
        );
        _container.RegisterDelegate<IPersistenceMetricsSource>(
            resolver => (IPersistenceMetricsSource)resolver.Resolve<IPersistenceService>(),
            Reuse.Singleton
        );
        _container.Register<IMetricProvider, GameLoopMetricsProvider>(Reuse.Singleton);
        _container.Register<IMetricProvider, NetworkMetricsProvider>(Reuse.Singleton);
        _container.Register<IMetricProvider, ScriptEngineMetricsProvider>(Reuse.Singleton);
        _container.Register<IMetricProvider, PersistenceMetricsProvider>(Reuse.Singleton);

        _container.RegisterMoongateService<IPersistenceService, PersistenceService>(110);
        _container.RegisterMoongateService<IGameLoopService, GameLoopService>(130);
        _container.RegisterMoongateService<ICommandSystemService, CommandSystemService>(131);
        _container.RegisterMoongateService<IConsoleCommandService, ConsoleCommandService>(132);
        _container.RegisterMoongateService<IMetricsCollectionService, MetricsCollectionService>(135);
        _container.RegisterMoongateService<INetworkService, NetworkService>(150);
        _container.RegisterMoongateService<IFileLoaderService, FileLoaderService>(120);
        _container.RegisterMoongateService<IGameEventScriptBridgeService, GameEventScriptBridgeService>(140);
        _container.RegisterMoongateService<IScriptEngineService, LuaScriptEngineService>(150);
    }

    private async Task StopAsync(List<IMoongateService> runningServices)
    {
        for (var i = runningServices.Count - 1; i >= 0; i--)
        {
            var service = runningServices[i];

            _logger.Information("Stopping {ServiceTypeFullName}", service.GetType().Name);
            await service.StopAsync();
        }
    }
}
