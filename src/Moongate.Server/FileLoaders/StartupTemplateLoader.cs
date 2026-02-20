using System.Text.Json;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.UO.Data.Interfaces.FileLoaders;
using Moongate.UO.Data.Interfaces.Templates;
using Serilog;

namespace Moongate.Server.FileLoaders;

/// <summary>
/// Loads startup templates from <c>templates/startup</c> into <see cref="IStartupTemplateService" />.
/// </summary>
public sealed class StartupTemplateLoader : IFileLoader
{
    private readonly ILogger _logger = Log.ForContext<StartupTemplateLoader>();
    private readonly DirectoriesConfig _directoriesConfig;
    private readonly IStartupTemplateService _startupTemplateService;

    public StartupTemplateLoader(DirectoriesConfig directoriesConfig, IStartupTemplateService startupTemplateService)
    {
        _directoriesConfig = directoriesConfig;
        _startupTemplateService = startupTemplateService;
    }

    public Task LoadAsync()
    {
        var templatesRootDirectory = Path.Combine(_directoriesConfig[DirectoryType.Templates], "startup");

        if (!Directory.Exists(templatesRootDirectory))
        {
            _logger.Warning("Startup templates directory not found: {Directory}", templatesRootDirectory);

            return Task.CompletedTask;
        }

        var templateFiles = Directory.GetFiles(templatesRootDirectory, "*.json", SearchOption.AllDirectories);

        if (templateFiles.Length == 0)
        {
            _logger.Warning("No startup template files found in {Directory}", templatesRootDirectory);

            return Task.CompletedTask;
        }

        _startupTemplateService.Clear();

        foreach (var templateFile in templateFiles)
        {
            JsonElement rootElement;

            try
            {
                using var document = JsonDocument.Parse(File.ReadAllText(templateFile));
                rootElement = document.RootElement.Clone();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load startup template file {TemplateFile}", templateFile);

                throw;
            }

            var templateId = Path.GetFileNameWithoutExtension(templateFile);
            _startupTemplateService.Upsert(templateId, rootElement);
        }

        _logger.Information(
            "Loaded {TemplateCount} startup templates from {FileCount} files",
            _startupTemplateService.Count,
            templateFiles.Length
        );

        return Task.CompletedTask;
    }
}
