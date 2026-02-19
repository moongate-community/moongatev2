using Moongate.Core.Types;
using Moongate.Core.Json;
using Moongate.Server.Bootstrap;
using Moongate.Server.Data.Config;
using Moongate.Server.Json;
using Moongate.Tests.TestSupport;
using Moongate.UO.Data.Files;

namespace Moongate.Tests.Server;

public class MoongateBootstrapConfigTests
{
    [Test]
    public void MoongateConfig_Defaults_ShouldInitializeMetricsConfig()
    {
        var config = new MoongateConfig();

        Assert.That(config.Metrics, Is.Not.Null);
        Assert.Multiple(
            () =>
            {
                Assert.That(config.Metrics.Enabled, Is.True);
                Assert.That(config.Metrics.IntervalMilliseconds, Is.EqualTo(1000));
                Assert.That(config.Metrics.LogEnabled, Is.True);
                Assert.That(config.Metrics.LogToConsole, Is.False);
            }
        );
    }

    [Test]
    public void MoongateConfig_SerializationRoundTrip_ShouldPreserveMetricsConfig()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        var config = new MoongateConfig
        {
            Metrics = new()
            {
                Enabled = true,
                IntervalMilliseconds = 2500,
                LogEnabled = false,
                LogToConsole = true
            }
        };

        JsonUtils.SerializeToFile(config, path, MoongateServerJsonContext.Default);
        var reloaded = JsonUtils.DeserializeFromFile<MoongateConfig>(path, MoongateServerJsonContext.Default);

        File.Delete(path);

        Assert.Multiple(
            () =>
            {
                Assert.That(reloaded.Metrics.Enabled, Is.True);
                Assert.That(reloaded.Metrics.IntervalMilliseconds, Is.EqualTo(2500));
                Assert.That(reloaded.Metrics.LogEnabled, Is.False);
                Assert.That(reloaded.Metrics.LogToConsole, Is.True);
            }
        );
    }

    [Test]
    public void Constructor_WhenUODirectoryIsMissing_ShouldThrowInvalidOperationException()
    {
        using var root = new TempDirectory();

        var ex = Assert.Throws<InvalidOperationException>(
            () => _ = new MoongateBootstrap(
                      new()
                      {
                          RootDirectory = root.Path,
                          UODirectory = string.Empty,
                          LogLevel = LogLevelType.Debug,
                          LogPacketData = false
                      }
                  )
        );

        Assert.That(ex!.Message, Is.EqualTo("UO Directory not configured."));
    }

    [Test]
    public void Constructor_WhenUODirectoryIsValid_ShouldSetUoFilesRootDir()
    {
        using var root = new TempDirectory();
        using var uo = new TempDirectory();

        using var bootstrap = new MoongateBootstrap(
            new()
            {
                RootDirectory = root.Path,
                UODirectory = uo.Path,
                LogLevel = LogLevelType.Debug,
                LogPacketData = false
            }
        );

        Assert.That(UoFiles.RootDir, Is.EqualTo(Path.GetFullPath(uo.Path)));
    }
}
