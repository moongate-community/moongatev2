using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.FileLoaders;
using Moongate.Tests.TestSupport;
using Moongate.UO.Data.Services.Templates;

namespace Moongate.Tests.Server;

public class StartupTemplateLoaderTests
{
    [Test]
    public void LoadAsync_WhenDirectoryMissing_ShouldNotThrow()
    {
        using var tempDirectory = new TempDirectory();
        var directoriesConfig = new DirectoriesConfig(tempDirectory.Path, DirectoryType.Templates);
        var service = new StartupTemplateService();
        var loader = new StartupTemplateLoader(directoriesConfig, service);

        Assert.That(async () => await loader.LoadAsync(), Throws.Nothing);
        Assert.That(service.Count, Is.Zero);
    }

    [Test]
    public async Task LoadAsync_WhenTemplateFilesExist_ShouldPopulateStartupTemplateService()
    {
        using var tempDirectory = new TempDirectory();
        var directoriesConfig = new DirectoriesConfig(
            tempDirectory.Path,
            DirectoryType.Data,
            DirectoryType.Templates,
            DirectoryType.Scripts,
            DirectoryType.Save,
            DirectoryType.Logs,
            DirectoryType.Cache,
            DirectoryType.Database
        );

        var startupDirectory = Path.Combine(directoriesConfig[DirectoryType.Templates], "startup");
        Directory.CreateDirectory(startupDirectory);

        await File.WriteAllTextAsync(
            Path.Combine(startupDirectory, "startup_base.json"),
            """
            {
              "startingGold": 1000,
              "baseBackpackItems": [ { "templateId": "Gold", "amount": 1000 } ]
            }
            """
        );

        var service = new StartupTemplateService();
        var loader = new StartupTemplateLoader(directoriesConfig, service);

        await loader.LoadAsync();

        Assert.Multiple(
            () =>
            {
                Assert.That(service.Count, Is.EqualTo(1));
                Assert.That(service.TryGet("startup_base", out var document), Is.True);
                Assert.That(document?.Content.GetProperty("startingGold").GetInt32(), Is.EqualTo(1000));
            }
        );
    }
}
