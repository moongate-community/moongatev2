using DryIoc;
using Moongate.Abstractions.Data.Internal;
using Moongate.Abstractions.Extensions;
using Moongate.Abstractions.Interfaces.Services.Base;
using Moongate.Server.Interfaces.Services;
using Moongate.Server.Services;
using Serilog;

namespace Moongate.Server.Bootstrap;

public class MoongateBootstrap
{
    private readonly Container _container = new();

    private readonly ILogger _logger = Log.ForContext<MoongateBootstrap>();

    public MoongateBootstrap()
    {
        RegisterServices();
    }

    private void RegisterServices()
    {
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
