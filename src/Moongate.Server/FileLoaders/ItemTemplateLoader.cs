using Moongate.Core.Data.Directories;
using Moongate.Core.Json;
using Moongate.Core.Types;
using Moongate.UO.Data.Interfaces.FileLoaders;
using Moongate.UO.Data.Interfaces.Templates;
using Moongate.UO.Data.Json.Context;
using Moongate.UO.Data.Templates.Items;
using Serilog;

namespace Moongate.Server.FileLoaders;

public sealed class ItemTemplateLoader : IFileLoader
{
    private readonly ILogger _logger = Log.ForContext<ItemTemplateLoader>();
    private readonly DirectoriesConfig _directoriesConfig;
    private readonly IItemTemplateService _itemTemplateService;

    public ItemTemplateLoader(DirectoriesConfig directoriesConfig, IItemTemplateService itemTemplateService)
    {
        _directoriesConfig = directoriesConfig;
        _itemTemplateService = itemTemplateService;
    }

    public Task LoadAsync()
    {
        var templatesRootDirectory = Path.Combine(_directoriesConfig[DirectoryType.Templates], "items");

        if (!Directory.Exists(templatesRootDirectory))
        {
            _logger.Warning("Item templates directory not found: {Directory}", templatesRootDirectory);

            return Task.CompletedTask;
        }

        var templateFiles = Directory.GetFiles(templatesRootDirectory, "*.json", SearchOption.AllDirectories);

        if (templateFiles.Length == 0)
        {
            _logger.Warning("No item template files found in {Directory}", templatesRootDirectory);

            return Task.CompletedTask;
        }

        _itemTemplateService.Clear();

        var loadedTemplateCount = 0;

        foreach (var templateFile in templateFiles)
        {
            ItemTemplateDefinitionBase[] templates;

            try
            {
                templates = JsonUtils.DeserializeFromFile<ItemTemplateDefinitionBase[]>(
                    templateFile,
                    MoongateUOTemplateJsonContext.Default
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load item template file {TemplateFile}", templateFile);

                throw;
            }

            var itemTemplates = templates.OfType<ItemTemplateDefinition>().ToList();
            _itemTemplateService.UpsertRange(itemTemplates);
            loadedTemplateCount += itemTemplates.Count;
        }

        _logger.Information(
            "Loaded {TemplateCount} item templates from {FileCount} files",
            loadedTemplateCount,
            templateFiles.Length
        );

        return Task.CompletedTask;
    }
}
