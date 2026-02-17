using Moongate.Core.Data.Directories;
using Moongate.Core.Json;
using Moongate.Core.Types;
using Moongate.UO.Data.Expansions;
using Moongate.UO.Data.Interfaces.FileLoaders;
using Moongate.UO.Data.Json.Context;
using Serilog;

namespace Moongate.Server.FileLoaders;

public class ExpansionLoader : IFileLoader
{
    private readonly DirectoriesConfig _directoriesConfig;
    private readonly ILogger _logger = Log.ForContext<ExpansionLoader>();

    public ExpansionLoader(DirectoriesConfig directoriesConfig)
        => _directoriesConfig = directoriesConfig;

    public async Task LoadAsync()
    {
        var filePath = Path.Combine(_directoriesConfig[DirectoryType.Data], "expansions.json");

        ExpansionInfo.Table = JsonUtils.DeserializeFromFile<ExpansionInfo[]>(
            filePath,
            MoongateUOJsonSerializationContext.Default
        );
    }
}
