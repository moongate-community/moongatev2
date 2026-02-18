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
using Moongate.Scripting.Data.Config;
using Moongate.Scripting.Extensions.Scripts;
using Moongate.Scripting.Interfaces;
using Moongate.Scripting.Modules;
using Moongate.Scripting.Services;
using Moongate.Server.Data.Config;
using Moongate.Server.FileLoaders;
using Moongate.Server.Http;
using Moongate.Server.Http.Interfaces;
using Moongate.Server.Interfaces.Services;
using Moongate.Server.Json;
using Moongate.Server.Services;
using Moongate.UO.Data.Files;
using Serilog;
using Serilog.Filters;

namespace Moongate.Server.Bootstrap;

public sealed class MoongateBootstrap : IDisposable
{
    private readonly Container _container = new();

    private ILogger _logger;

    private DirectoriesConfig _directoriesConfig;

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
        RegisterScriptModules();
        RegisterServices();
        RegisterFileLoaders();
    }

    private void RegisterHttpServer()
    {
        if (_moongateConfig.Http.IsEnabled)
        {
            _container.RegisterMoongateService<IMoongateHttpService, MoongateHttpService>(200);
            _logger.Information("HTTP Server enabled.");

            var httpServiceOptions = new MoongateHttpServiceOptions()
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

    private void CheckUODirectory()
    {
        if (string.IsNullOrWhiteSpace(_moongateConfig.UODirectory))
        {
            _logger.Error("UO Directory not configured.");

            throw new InvalidOperationException("UO Directory not configured.");
        }

        UoFiles.RootDir = _moongateConfig.UODirectory.ResolvePathAndEnvs();
        UoFiles.ReLoadDirectory();
        _logger.Information("UO Directory configured in {UODirectory}", UoFiles.RootDir);
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

            _logger.Information("Starting {ServiceTypeFullName}", serviceRegistration.ServiceType.Name);
            await instance.StartAsync();
            runningServices.Add(instance);
        }

        _logger.Information("Server started in {StartupTime} ms", Stopwatch.GetElapsedTime(startTime).TotalMilliseconds);
        _logger.Information("Moongate server is running. Press Ctrl+C to stop.");

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.Information("Shutdown requested.");
        }

        await StopAsync(runningServices);
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

    private void CreateLogger()
    {
        var appLogPath = Path.Combine(_directoriesConfig[DirectoryType.Logs], "moongate-.log");
        var packetLogPath = Path.Combine(_directoriesConfig[DirectoryType.Logs], "packets-.log");
        var configuration = new LoggerConfiguration()
                            .MinimumLevel
                            .Is(_moongateConfig.LogLevel.ToSerilogLogLevel())
                            .WriteTo
                            .Console()
                            .WriteTo
                            .File(
                                appLogPath,
                                rollingInterval: RollingInterval.Day
                            );

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

    private void RegisterServices()
    {
        _container.RegisterInstance(_moongateConfig);
        _container.RegisterInstance(_directoriesConfig);
        _container.Register<IMessageBusService, MessageBusService>(Reuse.Singleton);
        _container.Register<IGameEventBusService, GameEventBusService>(Reuse.Singleton);
        _container.Register<IOutgoingPacketQueue, OutgoingPacketQueue>(Reuse.Singleton);
        _container.Register<IPacketDispatchService, PacketDispatchService>(Reuse.Singleton);
        _container.Register<IGameNetworkSessionService, GameNetworkSessionService>(Reuse.Singleton);
        _container.RegisterMoongateService<IGameLoopService, GameLoopService>(130);
        _container.RegisterMoongateService<INetworkService, NetworkService>(150);
        _container.RegisterMoongateService<IFileLoaderService, FileLoaderService>(120);
        _container.RegisterMoongateService<IScriptEngineService, LuaScriptEngineService>(150);
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

    private void EnsureDataAssets()
    {
        var sourceDataDirectory = Path.Combine(AppContext.BaseDirectory, "Assets", "data");
        var destinationDataDirectory = _directoriesConfig[DirectoryType.Data];

        DataAssetsBootstrapper.EnsureDataAssets(sourceDataDirectory, destinationDataDirectory, _logger);
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

    public void Dispose()
    {
        _container.Dispose();
        GC.SuppressFinalize(this);
    }
}
