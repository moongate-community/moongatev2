using Moongate.Server.FileLoaders;
using Moongate.Server.Interfaces.Services.Files;

namespace Moongate.Server.Bootstrap.Internal;

/// <summary>
/// Registers built-in file loaders in the configured file loader service.
/// </summary>
internal static class BootstrapFileLoaderRegistration
{
    public static void Register(IFileLoaderService fileLoaderService)
    {
        fileLoaderService.AddFileLoader<ClientVersionLoader>();
        fileLoaderService.AddFileLoader<SkillLoader>();
        fileLoaderService.AddFileLoader<ExpansionLoader>();
        fileLoaderService.AddFileLoader<BodyDataLoader>();
        fileLoaderService.AddFileLoader<ProfessionsLoader>();
        fileLoaderService.AddFileLoader<MultiDataLoader>();
        fileLoaderService.AddFileLoader<RaceLoader>();
        fileLoaderService.AddFileLoader<TileDataLoader>();
        fileLoaderService.AddFileLoader<MapLoader>();
        fileLoaderService.AddFileLoader<CliLocLoader>();
        fileLoaderService.AddFileLoader<ContainersDataLoader>();
        fileLoaderService.AddFileLoader<ItemTemplateLoader>();
        fileLoaderService.AddFileLoader<MobileTemplateLoader>();
        fileLoaderService.AddFileLoader<StartupTemplateLoader>();
        fileLoaderService.AddFileLoader<TemplateValidationLoader>();
        fileLoaderService.AddFileLoader<RegionDataLoader>();
        fileLoaderService.AddFileLoader<WeatherDataLoader>();
        fileLoaderService.AddFileLoader<NamesLoader>();
    }
}
