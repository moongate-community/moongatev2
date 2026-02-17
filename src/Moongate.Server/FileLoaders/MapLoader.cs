using Moongate.UO.Data.Interfaces.FileLoaders;
using Moongate.UO.Data.Maps;
using Moongate.UO.Data.Types;
using Serilog;

namespace Moongate.Server.FileLoaders;

public class MapLoader : IFileLoader
{
    private readonly ILogger _logger = Log.ForContext<MapLoader>();

    public async Task LoadAsync()
    {
        Map.RegisterMap(0, 0, 0, 7168, 4096, SeasonType.Desolation, "Felucca", MapRules.FeluccaRules);

        Map.RegisterMap(1, 1, 1, 7168, 4096, SeasonType.Spring, "Trammel", MapRules.TrammelRules);

        Map.RegisterMap(2, 2, 2, 2304, 1600, SeasonType.Summer, "Ilshenar", MapRules.TrammelRules);

        Map.RegisterMap(3, 3, 3, 2560, 2048, SeasonType.Summer, "Malas", MapRules.TrammelRules);

        Map.RegisterMap(4, 4, 4, 1448, 1448, SeasonType.Summer, "Tokuno", MapRules.TrammelRules);

        Map.RegisterMap(5, 5, 5, 1280, 4096, SeasonType.Summer, "TerMur", MapRules.TrammelRules);

        Map.RegisterMap(0x7F, 0x7F, 0, 1, 1, SeasonType.Spring, "Internal", MapRules.Internal);

        _logger.Information("Loaded {Count} maps", Map.MapCount);
    }
}
