using DryIoc;
using Moongate.Abstractions.Data.Internal;
using Moongate.Abstractions.Extensions;
using Moongate.Abstractions.Interfaces.Services.Base;
using Moongate.Core.Data.Directories;
using Moongate.Core.Extensions.Directories;
using Moongate.Core.Extensions.Logger;
using Moongate.Core.Types;
using Moongate.Server.Data.Config;
using Moongate.Server.Interfaces.Services;
using Moongate.Server.Services;
using Serilog;
using Serilog.Filters;

namespace Moongate.Server.Bootstrap;

public class MoongateBootstrap
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
        Console.WriteLine("Root Directory: " + _directoriesConfig.Root);
        RegisterServices();
    }

    private void CheckDirectoryConfig()
    {
        if (string.IsNullOrWhiteSpace(_moongateConfig.RootDirectory))
        {
            _moongateConfig.RootDirectory = Environment.GetEnvironmentVariable("MOONGATE_ROOT_DIRECTORY") ??
                                            Path.Combine(Directory.GetCurrentDirectory(), "moongate");


        }

        _moongateConfig.RootDirectory = _moongateConfig.RootDirectory.ResolvePathAndEnvs();

        _directoriesConfig = new DirectoriesConfig(_moongateConfig.RootDirectory, Enum.GetNames<DirectoryType>());
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
        _container.Register<IOutgoingPacketQueue, OutgoingPacketQueue>(Reuse.Singleton);
        _container.Register<IPacketDispatchService, PacketDispatchService>(Reuse.Singleton);
        _container.Register<IGameNetworkSessionService, GameNetworkSessionService>(Reuse.Singleton);
        _container.RegisterMoongateService<IGameLoopService, GameLoopService>(100);
        _container.RegisterMoongateService<INetworkService, NetworkService>(99);
        _container.RegisterDelegate<IGamePacketIngress>(
            static resolver => resolver.Resolve<IGameLoopService>(),
            Reuse.Singleton
        );
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
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

    private async Task StopAsync(IReadOnlyList<IMoongateService> runningServices)
    {
        for (var i = runningServices.Count - 1; i >= 0; i--)
        {
            var service = runningServices[i];

            _logger.Information("Stopping {ServiceTypeFullName}", service.GetType().Name);
            await service.StopAsync();
        }
    }
}
