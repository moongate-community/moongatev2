using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.FileLoaders;
using Moongate.Tests.TestSupport;
using Moongate.UO.Data.Services.Templates;

namespace Moongate.Tests.Server;

public class MobileTemplateLoaderTests
{
    [Test]
    public void LoadAsync_WhenDirectoryMissing_ShouldNotThrow()
    {
        using var tempDirectory = new TempDirectory();
        var directoriesConfig = new DirectoriesConfig(tempDirectory.Path, DirectoryType.Templates);
        var mobileTemplateService = new MobileTemplateService();
        var loader = new MobileTemplateLoader(directoriesConfig, mobileTemplateService);

        Assert.That(async () => await loader.LoadAsync(), Throws.Nothing);
        Assert.That(mobileTemplateService.Count, Is.Zero);
    }

    [Test]
    public async Task LoadAsync_WhenTemplateFilesExist_ShouldPopulateTemplateService()
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

        var mobilesDirectory = Path.Combine(directoriesConfig[DirectoryType.Templates], "mobiles");
        Directory.CreateDirectory(mobilesDirectory);

        var filePath = Path.Combine(mobilesDirectory, "orcs.json");
        await File.WriteAllTextAsync(
            filePath,
            """
            [
              {
                "type": "mobile",
                "id": "orc_warrior",
                "name": "Orc Warrior",
                "category": "monsters",
                "description": "Orc melee unit",
                "tags": ["orc"],
                "body": "0x11",
                "skinHue": "hue(779:790)",
                "hairHue": 0,
                "hairStyle": 0,
                "brain": "aggressive_orc"
              }
            ]
            """
        );

        var mobileTemplateService = new MobileTemplateService();
        var loader = new MobileTemplateLoader(directoriesConfig, mobileTemplateService);

        await loader.LoadAsync();

        Assert.Multiple(
            () =>
            {
                Assert.That(mobileTemplateService.Count, Is.EqualTo(1));
                Assert.That(mobileTemplateService.TryGet("orc_warrior", out var definition), Is.True);
                Assert.That(definition?.Body, Is.EqualTo(0x11));
                Assert.That(definition?.SkinHue.IsRange, Is.True);
                Assert.That(definition?.Brain, Is.EqualTo("aggressive_orc"));
            }
        );
    }
}
