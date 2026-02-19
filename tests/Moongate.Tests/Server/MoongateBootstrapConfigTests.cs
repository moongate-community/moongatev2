using Moongate.Core.Types;
using Moongate.Server.Bootstrap;
using Moongate.Tests.TestSupport;
using Moongate.UO.Data.Files;

namespace Moongate.Tests.Server;

public class MoongateBootstrapConfigTests
{
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
