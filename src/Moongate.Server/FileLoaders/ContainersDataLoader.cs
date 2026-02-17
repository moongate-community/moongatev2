using Moongate.Core.Data.Directories;
using Moongate.Core.Json;
using Moongate.Core.Types;
using Moongate.UO.Data.Containers;
using Moongate.UO.Data.Interfaces.FileLoaders;
using Moongate.UO.Data.Json;
using Moongate.UO.Data.Json.Context;
using Serilog;

namespace Moongate.Server.FileLoaders;

public class ContainersDataLoader : IFileLoader
{
    private readonly DirectoriesConfig _directoriesConfig;
    private readonly ILogger _logger = Log.ForContext<ContainersDataLoader>();

    public ContainersDataLoader(DirectoriesConfig directoriesConfig)
        => _directoriesConfig = directoriesConfig;

    public async Task LoadAsync()
    {
        var containersDirectory = Path.Combine(_directoriesConfig[DirectoryType.Data], "containers");

        var files = Directory.GetFiles(containersDirectory, "*.json");

        foreach (var containerFile in files)
        {
            var container = JsonUtils.DeserializeFromFile<JsonContainerSize[]>(
                containerFile,
                MoongateUOJsonSerializationContext.Default
            );

            foreach (var containerSize in container)
            {
                _logger.Debug("Adding {JsonContainerSize}", containerSize);
                ContainerLayoutSystem.ContainerSizes.Add(
                    containerSize.ItemId,
                    new(containerSize.Width, containerSize.Height, containerSize.Name)
                );
            }
        }
    }
}
