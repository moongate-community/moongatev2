using Moongate.Core.Data.Directories;
using Moongate.Core.Json;
using Moongate.Core.Types;
using Moongate.UO.Data.Context;
using Moongate.UO.Data.Interfaces.FileLoaders;
using Moongate.UO.Data.Json.Context;
using Moongate.UO.Data.Skills;
using Serilog;

namespace Moongate.Server.FileLoaders;

public class SkillLoader : IFileLoader
{
    private readonly DirectoriesConfig _directoriesConfig;

    private readonly ILogger _logger = Log.ForContext<SkillLoader>();

    public SkillLoader(DirectoriesConfig directoriesConfig)
        => _directoriesConfig = directoriesConfig;

    public async Task LoadAsync()
    {
        UOContext.SkillsInfo =
            JsonUtils.DeserializeFromFile<SkillInfo[]>(
                Path.Combine(_directoriesConfig[DirectoryType.Data], "skills.json"),
                MoongateUOJsonSerializationContext.Default
            );

        SkillInfo.Table = UOContext.SkillsInfo;

        _logger.Information("Loaded {Count} skills from skills.json", UOContext.SkillsInfo?.Length ?? 0);
    }
}
