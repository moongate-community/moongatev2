using Moongate.Core.Data.Directories;
using Moongate.Core.Json;
using Moongate.Core.Types;
using Moongate.UO.Data.Interfaces.FileLoaders;
using Moongate.UO.Data.Json.Context;
using Moongate.UO.Data.Json.Names;
using Serilog;

namespace Moongate.Server.FileLoaders;

public class NamesLoader : IFileLoader
{
    private readonly DirectoriesConfig _directoriesConfig;

    // private readonly INameService _nameService;
    private readonly ILogger _logger = Log.ForContext<NamesLoader>();

    public NamesLoader(DirectoriesConfig directoriesConfig)
        => _directoriesConfig = directoriesConfig;

    // _nameService = nameService;
    public async Task LoadAsync()
    {
        var filePath = Path.Combine(_directoriesConfig[DirectoryType.Data], "names");

        var files = Directory.GetFiles(filePath, "*.json", SearchOption.AllDirectories);

        _logger.Information("Found {Count} names files", files.Length);

        foreach (var file in files)
        {
            try
            {
                var jsonNameContent = JsonUtils.DeserializeFromFile<JsonNameDef[]>(
                    file,
                    MoongateUOJsonSerializationContext.Default
                );

                foreach (var nameContext in jsonNameContent)
                {
                    //_nameService.AddNames(nameContext.Type, nameContext.Names.ToArray());
                }
            }
            catch (Exception ex)
            {
                // Log the error or handle it as needed
                Console.WriteLine($"Error loading names from file {file}: {ex.Message}");
            }
        }
    }
}
