using DryIoc;
using Moongate.Server.Interfaces.Services.GameLoop;
using Moongate.Server.Interfaces.Services.Metrics;
using Moongate.Server.Interfaces.Services.Network;
using Moongate.Server.Interfaces.Services.Persistence;
using Moongate.Server.Interfaces.Services.Timing;
using Moongate.Server.Services.Metrics;
using Moongate.Server.Services.Metrics.Providers;

namespace Moongate.Server.Extensions.Bootstrap;

/// <summary>
/// Registers metrics-related services and providers.
/// </summary>
public static class AddBootstrapMetricsServicesExtension
{
    /// <summary>
    /// Registers metrics sources and provider implementations.
    /// </summary>
    public static Container AddBootstrapMetricsServices(this Container container)
    {
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
        container.RegisterDelegate<ITimerMetricsSource>(
            resolver => (ITimerMetricsSource)resolver.Resolve<ITimerService>(),
            Reuse.Singleton
        );
        container.Register<IMetricsHttpSnapshotFactory, MetricsHttpSnapshotFactory>(Reuse.Singleton);
        container.Register<IMetricProvider, GameLoopMetricsProvider>(Reuse.Singleton);
        container.Register<IMetricProvider, NetworkMetricsProvider>(Reuse.Singleton);
        container.Register<IMetricProvider, ScriptEngineMetricsProvider>(Reuse.Singleton);
        container.Register<IMetricProvider, PersistenceMetricsProvider>(Reuse.Singleton);
        container.Register<IMetricProvider, TimerMetricsProvider>(Reuse.Singleton);

        return container;
    }
}
