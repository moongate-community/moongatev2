using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.FileLoaders;
using Moongate.Tests.TestSupport;
using Moongate.UO.Data.Services.Names;

namespace Moongate.Tests.Server;

public class NamesLoaderTests
{
    [Test]
    public async Task LoadAsync_WhenNamesFilesExist_ShouldPopulateNameService()
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

        var namesDirectory = Path.Combine(directoriesConfig[DirectoryType.Data], "names");
        Directory.CreateDirectory(namesDirectory);

        await File.WriteAllTextAsync(
            Path.Combine(namesDirectory, "names.json"),
            """
            [
              {
                "type": "tokuno male",
                "names": [ "Dosyaku", "Warimoto" ]
              }
            ]
            """
        );

        var nameService = new NameService();
        var loader = new NamesLoader(directoriesConfig, nameService);

        await loader.LoadAsync();

        var generated = nameService.GenerateName("tokuno male");
        Assert.That(generated, Is.Not.Empty);
    }
}
