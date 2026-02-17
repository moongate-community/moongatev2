using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.FileLoaders;
using System.Text.Json;

namespace Moongate.Tests.Server;

public class JsonFileLoadersNegativeTests
{
    [Test]
    public void RegionDataLoader_WhenRegionsDirectoryDoesNotExist_ShouldThrowDirectoryNotFoundException()
    {
        using var temp = new TempDirectory();
        var directories = new DirectoriesConfig(temp.Path, Enum.GetNames<DirectoryType>());
        var loader = new RegionDataLoader(directories);

        Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await loader.LoadAsync());
    }

    [Test]
    public void WeatherDataLoader_WhenWeatherDirectoryDoesNotExist_ShouldThrowDirectoryNotFoundException()
    {
        using var temp = new TempDirectory();
        var directories = new DirectoriesConfig(temp.Path, Enum.GetNames<DirectoryType>());
        var loader = new WeatherDataLoader(directories);

        Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await loader.LoadAsync());
    }

    [Test]
    public void ContainersDataLoader_WhenJsonIsInvalid_ShouldThrowJsonException()
    {
        using var temp = new TempDirectory();
        var directories = new DirectoriesConfig(temp.Path, Enum.GetNames<DirectoryType>());
        var containersPath = Path.Combine(directories[DirectoryType.Data], "containers");
        Directory.CreateDirectory(containersPath);
        File.WriteAllText(Path.Combine(containersPath, "broken.json"), "{ not-a-json }");

        var loader = new ContainersDataLoader(directories);

        Assert.ThrowsAsync<JsonException>(async () => await loader.LoadAsync());
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "moongate-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Path, true);
            }
            catch
            {
                // best-effort temp cleanup
            }
        }
    }
}
