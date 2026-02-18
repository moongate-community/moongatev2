using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Http;
using Moongate.Tests.Server.Http.Support;
using Moongate.Tests.TestSupport;

namespace Moongate.Tests.Server.Http;

public class MoongateHttpServiceOptionsTests
{
    [Test]
    public void Constructor_WhenOptionsAreNull_ShouldThrowArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => _ = new MoongateHttpService(null!));

        Assert.That(ex!.ParamName, Is.EqualTo("options"));
    }

    [Test]
    public void Constructor_WhenServiceMappingsAreMissing_ShouldCreateInstance()
    {
        using var temp = new TempDirectory();
        var directories = new DirectoriesConfig(temp.Path, Enum.GetNames<DirectoryType>());

        var instance = new MoongateHttpService(
            new MoongateHttpServiceOptions { DirectoriesConfig = directories }
        );

        Assert.That(instance, Is.Not.Null);
    }

    [Test]
    public void Constructor_WhenServiceMappingsAreEmpty_ShouldCreateInstance()
    {
        using var temp = new TempDirectory();
        var directories = new DirectoriesConfig(temp.Path, Enum.GetNames<DirectoryType>());

        var instance = new MoongateHttpService(
            new MoongateHttpServiceOptions
            {
                ServiceMappings = new Dictionary<Type, Type>(),
                DirectoriesConfig = directories
            }
        );

        Assert.That(instance, Is.Not.Null);
    }

    [Test]
    public void Constructor_WhenDirectoriesConfigIsMissing_ShouldThrowArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => _ = new MoongateHttpService(
                new MoongateHttpServiceOptions
                {
                    ServiceMappings = new Dictionary<Type, Type>
                    {
                        { typeof(IMoongateHttpOptionsDummyService), typeof(MoongateHttpOptionsDummyService) }
                    }
                }
            )
        );

        Assert.That(ex!.Message, Does.Contain("DirectoriesConfig"));
    }

    [Test]
    public void Constructor_WhenPortIsOutOfRange_ShouldThrowArgumentOutOfRangeException()
    {
        using var temp = new TempDirectory();
        var directories = new DirectoriesConfig(temp.Path, Enum.GetNames<DirectoryType>());

        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => _ = new MoongateHttpService(
                new MoongateHttpServiceOptions
                {
                    ServiceMappings = new Dictionary<Type, Type>
                    {
                        { typeof(IMoongateHttpOptionsDummyService), typeof(MoongateHttpOptionsDummyService) }
                    },
                    DirectoriesConfig = directories,
                    Port = 0
                }
            )
        );

        Assert.That(ex!.Message, Does.Contain("1-65535"));
    }

    [Test]
    public void Constructor_WhenOptionsAreValid_ShouldCreateInstance()
    {
        using var temp = new TempDirectory();
        var directories = new DirectoriesConfig(temp.Path, Enum.GetNames<DirectoryType>());

        var instance = new MoongateHttpService(
            new MoongateHttpServiceOptions
            {
                ServiceMappings = new Dictionary<Type, Type>
                {
                    { typeof(IMoongateHttpOptionsDummyService), typeof(MoongateHttpOptionsDummyService) }
                },
                DirectoriesConfig = directories,
                Port = 8088
            }
        );

        Assert.That(instance, Is.Not.Null);
    }

    [Test]
    public void Constructor_WhenConfigureAppIsNull_ShouldCreateInstance()
    {
        using var temp = new TempDirectory();
        var directories = new DirectoriesConfig(temp.Path, Enum.GetNames<DirectoryType>());

        var instance = new MoongateHttpService(
            new MoongateHttpServiceOptions
            {
                ServiceMappings = new Dictionary<Type, Type>
                {
                    { typeof(IMoongateHttpOptionsDummyService), typeof(MoongateHttpOptionsDummyService) }
                },
                DirectoriesConfig = directories,
                ConfigureApp = null
            }
        );

        Assert.That(instance, Is.Not.Null);
    }

}
