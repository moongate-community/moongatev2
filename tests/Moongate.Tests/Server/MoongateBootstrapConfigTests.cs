using Moongate.Core.Types;
using Moongate.Server.Bootstrap;
using Moongate.Server.Data.Config;
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
                new MoongateConfig
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
            new MoongateConfig
            {
                RootDirectory = root.Path,
                UODirectory = uo.Path,
                LogLevel = LogLevelType.Debug,
                LogPacketData = false
            }
        );

        Assert.That(UoFiles.RootDir, Is.EqualTo(Path.GetFullPath(uo.Path)));
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
