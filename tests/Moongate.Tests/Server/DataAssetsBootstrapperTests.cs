using Moongate.Server.Bootstrap;
using Moongate.Tests.TestSupport;
using Serilog;

namespace Moongate.Tests.Server;

public class DataAssetsBootstrapperTests
{
    [Test]
    public void EnsureDataAssets_WhenDestinationFileExists_ShouldNotOverwrite()
    {
        using var source = new TempDirectory();
        using var destination = new TempDirectory();

        var relativePath = Path.Combine("weather", "weather.json");
        var sourceFile = Path.Combine(source.Path, relativePath);
        var destinationFile = Path.Combine(destination.Path, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(sourceFile)!);
        Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);

        File.WriteAllText(sourceFile, "source");
        File.WriteAllText(destinationFile, "existing");

        var copied = DataAssetsBootstrapper.EnsureDataAssets(
            source.Path,
            destination.Path,
            new LoggerConfiguration().CreateLogger()
        );

        Assert.That(copied, Is.EqualTo(0));
        Assert.That(File.ReadAllText(destinationFile), Is.EqualTo("existing"));
    }

    [Test]
    public void EnsureDataAssets_WhenFilesAreMissing_ShouldCopyFilesPreservingStructure()
    {
        using var source = new TempDirectory();
        using var destination = new TempDirectory();

        var sourceFile = Path.Combine(source.Path, "regions", "regions.json");
        Directory.CreateDirectory(Path.GetDirectoryName(sourceFile)!);
        File.WriteAllText(sourceFile, "{\"ok\":true}");

        var copied = DataAssetsBootstrapper.EnsureDataAssets(
            source.Path,
            destination.Path,
            new LoggerConfiguration().CreateLogger()
        );

        Assert.That(copied, Is.EqualTo(1));

        var destinationFile = Path.Combine(destination.Path, "regions", "regions.json");
        Assert.That(File.Exists(destinationFile), Is.True);
        Assert.That(File.ReadAllText(destinationFile), Is.EqualTo("{\"ok\":true}"));
    }
}
