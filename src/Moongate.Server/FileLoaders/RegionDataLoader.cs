using Moongate.Core.Data.Directories;
using Moongate.Core.Json;
using Moongate.Core.Types;
using Moongate.UO.Data.Interfaces.FileLoaders;
using Moongate.UO.Data.Json.Context;
using Moongate.UO.Data.Json.Regions;
using Serilog;

namespace Moongate.Server.FileLoaders;

public class RegionDataLoader : IFileLoader
{
    private readonly DirectoriesConfig _directoriesConfig;

    private readonly ILogger _logger = Log.ForContext<RegionDataLoader>();

    // private readonly ISpatialWorldService _spatialWorldService;

    public RegionDataLoader(DirectoriesConfig directoriesConfig) //), ISpatialWorldService spatialWorldService)
        => _directoriesConfig = directoriesConfig;

    // _spatialWorldService = spatialWorldService;
    public async Task LoadAsync()
    {
        var regionDataDirectory = Path.Combine(_directoriesConfig[DirectoryType.Data], "regions");

        var regionFiles = Directory.GetFiles(regionDataDirectory, "*.json");

        foreach (var regionFile in regionFiles)
        {
            var regionData = JsonUtils.DeserializeFromFile<JsonRegionWrap>(
                regionFile,
                MoongateUOJsonSerializationContext.Default
            );

            foreach (var dataRegion in regionData.Regions)
            {
                // _spatialWorldService.AddRegion(dataRegion);
            }

            // _spatialWorldService.AddMusics(regionData.MusicLists);

            _logger.Information(
                "Loaded {RegionCount} regions from file: {FilePath}",
                regionData.Regions.Count,
                regionFile
            );
        }
    }
}
