using DryIoc;
using Moongate.Core.Data.Directories;
using Moongate.Server.Data.Config;
using Moongate.Server.Interfaces.Services.Console;

namespace Moongate.Server.Extensions.Bootstrap;

/// <summary>
/// Registers shared bootstrap instances in the DI container.
/// </summary>
public static class AddBootstrapInstancesExtension
{
    /// <summary>
    /// Registers configuration and singleton instance objects required by server services.
    /// </summary>
    public static Container AddBootstrapInstances(
        this Container container,
        MoongateConfig config,
        DirectoriesConfig directoriesConfig,
        TimerServiceConfig timerServiceConfig,
        IConsoleUiService consoleUiService
    )
    {
        container.RegisterInstance(config);
        container.RegisterInstance(config.Metrics);
        container.RegisterInstance(directoriesConfig);
        container.RegisterInstance(timerServiceConfig);
        container.RegisterInstance(consoleUiService);

        return container;
    }
}
