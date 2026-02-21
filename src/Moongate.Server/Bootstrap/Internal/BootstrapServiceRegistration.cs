using DryIoc;
using Moongate.Core.Data.Directories;
using Moongate.Server.Data.Config;
using Moongate.Server.Extensions.Bootstrap;
using Moongate.Server.Interfaces.Services.Console;

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
            WheelSize = Math.Max(1, config.Game.TimerWheelSize),
            IdleCpuEnabled = config.Game.IdleCpuEnabled,
            IdleSleepMilliseconds = Math.Max(1, config.Game.IdleSleepMilliseconds)
        };

        container.AddBootstrapInstances(config, directoriesConfig, timerServiceConfig, consoleUiService)
                 .AddBootstrapCoreServices()
                 .AddBootstrapMetricsServices()
                 .AddBootstrapHostedServices();
    }
}
