using Moongate.Core.Data.Directories;
using Moongate.Core.Json;
using Moongate.Core.Types;
using Moongate.UO.Data.Interfaces.FileLoaders;
using Moongate.UO.Data.Json.Context;
using Moongate.UO.Data.Json.Weather;
using Serilog;

namespace Moongate.Server.FileLoaders;

public class WeatherDataLoader : IFileLoader
{
    private readonly DirectoriesConfig _directoriesConfig;

    private readonly ILogger _logger = Log.ForContext<WeatherDataLoader>();

    public WeatherDataLoader(DirectoriesConfig directoriesConfig)
        => _directoriesConfig = directoriesConfig;

    public async Task LoadAsync()
    {
        var weatherDataDirectory = Path.Combine(_directoriesConfig[DirectoryType.Data], "weather");

        var weatherTypes = Directory.GetFiles(weatherDataDirectory, "*.json");

        foreach (var weatherFile in weatherTypes)
        {
            var weatherData = JsonUtils.DeserializeFromFile<JsonWeatherWrap>(
                weatherFile,
                MoongateUOJsonSerializationContext.Default
            );
            _logger.Information(
                "Loaded {WeatherType} weather from file: {FilePath}",
                weatherData.WeatherTypes.Count,
                weatherFile
            );
        }
    }
}
