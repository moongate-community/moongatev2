using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.FileLoaders;
using Moongate.Tests.TestSupport;
using Moongate.UO.Data.Services.Templates;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.Server;

public class ItemTemplateLoaderTests
{
    [Test]
    public void LoadAsync_WhenItemsDirectoryDoesNotExist_ShouldNotThrow()
    {
        using var tempDirectory = new TempDirectory();
        var directoriesConfig = new DirectoriesConfig(tempDirectory.Path, DirectoryType.Templates);
        var itemTemplateService = new ItemTemplateService();
        var loader = new ItemTemplateLoader(directoriesConfig, itemTemplateService);

        Assert.That(async () => await loader.LoadAsync(), Throws.Nothing);
        Assert.That(itemTemplateService.Count, Is.Zero);
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

        var itemsDirectory = Path.Combine(directoriesConfig[DirectoryType.Templates], "items", "clothes");
        Directory.CreateDirectory(itemsDirectory);

        var filePath = Path.Combine(itemsDirectory, "startup.json");
        await File.WriteAllTextAsync(
            filePath,
            """
            [
              {
                "type": "item",
                "category": "clothes",
                "id": "item.startup.shirt",
                "name": "Startup Shirt",
                "description": "Starter shirt",
                "container": [],
                "dyeable": true,
                "goldValue": "0",
                "hue": "0",
                "isMovable": true,
                "itemId": "0x1517",
                "lootType": "Regular",
                "scriptId": "none",
                "stackable": false,
                "tags": [],
                "weight": 1.0
              }
            ]
            """
        );

        var itemTemplateService = new ItemTemplateService();
        var loader = new ItemTemplateLoader(directoriesConfig, itemTemplateService);

        await loader.LoadAsync();

        Assert.Multiple(
            () =>
            {
                Assert.That(itemTemplateService.Count, Is.EqualTo(1));
                Assert.That(itemTemplateService.TryGet("item.startup.shirt", out var template), Is.True);
                Assert.That(template?.ItemId, Is.EqualTo("0x1517"));
                Assert.That(template?.LootType, Is.EqualTo(LootType.Regular));
                Assert.That(template?.Hue.IsRange, Is.False);
                Assert.That(template?.Hue.Resolve(), Is.EqualTo(0));
                Assert.That(template?.GoldValue.IsDiceExpression, Is.False);
                Assert.That(template?.GoldValue.Resolve(), Is.EqualTo(0));
            }
        );
    }
}
