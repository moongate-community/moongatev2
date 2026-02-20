using Moongate.Core.Data.Directories;
using Moongate.Core.Json;
using Moongate.Core.Types;
using Moongate.UO.Data.Interfaces.FileLoaders;
using Moongate.UO.Data.Interfaces.Templates;
using Moongate.UO.Data.Json.Context;
using Moongate.UO.Data.Templates.Mobiles;
using Serilog;

namespace Moongate.Server.FileLoaders;

/// <summary>
/// Loads mobile templates from <c>templates/mobiles</c> into <see cref="IMobileTemplateService" />.
/// </summary>
public sealed class MobileTemplateLoader : IFileLoader
{
    private readonly ILogger _logger = Log.ForContext<MobileTemplateLoader>();
    private readonly DirectoriesConfig _directoriesConfig;
    private readonly IMobileTemplateService _mobileTemplateService;

    public MobileTemplateLoader(DirectoriesConfig directoriesConfig, IMobileTemplateService mobileTemplateService)
    {
        _directoriesConfig = directoriesConfig;
        _mobileTemplateService = mobileTemplateService;
    }

    public Task LoadAsync()
    {
        var templatesRootDirectory = Path.Combine(_directoriesConfig[DirectoryType.Templates], "mobiles");

        if (!Directory.Exists(templatesRootDirectory))
        {
            _logger.Warning("Mobile templates directory not found: {Directory}", templatesRootDirectory);

            return Task.CompletedTask;
        }

        var templateFiles = Directory.GetFiles(templatesRootDirectory, "*.json", SearchOption.AllDirectories);

        if (templateFiles.Length == 0)
        {
            _logger.Warning("No mobile template files found in {Directory}", templatesRootDirectory);

            return Task.CompletedTask;
        }

        _mobileTemplateService.Clear();

        var loadedTemplateCount = 0;

        foreach (var templateFile in templateFiles)
        {
            MobileTemplateDefinitionBase[] templates;

            try
            {
                templates = JsonUtils.DeserializeFromFile<MobileTemplateDefinitionBase[]>(
                    templateFile,
                    MoongateUOTemplateJsonContext.Default
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load mobile template file {TemplateFile}", templateFile);

                throw;
            }

            var mobileTemplates = templates.OfType<MobileTemplateDefinition>().ToList();
            _mobileTemplateService.UpsertRange(mobileTemplates);
            loadedTemplateCount += mobileTemplates.Count;
        }

        _logger.Information(
            "Loaded {TemplateCount} mobile templates from {FileCount} files",
            loadedTemplateCount,
            templateFiles.Length
        );

        return Task.CompletedTask;
    }
}
