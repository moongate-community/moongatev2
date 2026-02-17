using Moongate.UO.Data.Interfaces.FileLoaders;
using Moongate.UO.Data.Races;
using Moongate.UO.Data.Races.Base;
using Serilog;

namespace Moongate.Server.FileLoaders;

public class RaceLoader : IFileLoader
{
    private readonly ILogger _logger = Log.ForContext<RaceLoader>();

    public async Task LoadAsync()
    {
        /* Here we configure all races. Some notes:
         *
         * 1) The first 32 races are reserved for core use.
         * 2) Race 0x7F is reserved for core use.
         * 3) Race 0xFF is reserved for core use.
         * 4) Changing or removing any predefined races may cause server instability.
         */

        RaceDefinitions.RegisterRace(new Human(0, 0));
        RaceDefinitions.RegisterRace(new Elf(1, 1));
        RaceDefinitions.RegisterRace(new Gargoyle(2, 2));

        _logger.Information("Loaded {Count} races", 3);
    }
}
