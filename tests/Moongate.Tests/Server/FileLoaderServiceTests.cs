using DryIoc;
using Moongate.Server.Services;
using Moongate.Server.Services.Files;
using Moongate.Tests.Server.Support;

namespace Moongate.Tests.Server;

public class FileLoaderServiceTests
{
    public static List<string> ExecutionLog { get; } = [];

    [Test]
    public async Task AddFileLoader_WhenCalledTwiceForSameType_ShouldExecuteOnlyOnce()
    {
        using var container = new Container();
        var service = new FileLoaderService(container);

        service.AddFileLoader<FileLoaderServiceTestLoaderA>();
        service.AddFileLoader<FileLoaderServiceTestLoaderA>();

        await service.ExecuteLoadersAsync();

        Assert.That(ExecutionLog, Is.EqualTo(["A"]));
    }

    [Test]
    public void AddFileLoader_WhenTypeIsNotRegistered_ShouldRegisterItInContainer()
    {
        using var container = new Container();
        var service = new FileLoaderService(container);

        Assert.That(container.IsRegistered<FileLoaderServiceTestLoaderA>(), Is.False);

        service.AddFileLoader<FileLoaderServiceTestLoaderA>();

        Assert.That(container.IsRegistered<FileLoaderServiceTestLoaderA>(), Is.True);
    }

    [Test]
    public async Task ExecuteLoadersAsync_ShouldRunLoadersInInsertionOrder()
    {
        using var container = new Container();
        var service = new FileLoaderService(container);

        service.AddFileLoader<FileLoaderServiceTestLoaderA>();
        service.AddFileLoader<FileLoaderServiceTestLoaderB>();

        await service.ExecuteLoadersAsync();

        Assert.That(ExecutionLog, Is.EqualTo(["A", "B"]));
    }

    [Test]
    public void ExecuteLoadersAsync_WhenLoaderThrows_ShouldPropagateException()
    {
        using var container = new Container();
        var service = new FileLoaderService(container);

        service.AddFileLoader<FileLoaderServiceTestLoaderThrows>();

        Assert.ThrowsAsync<InvalidOperationException>(async () => await service.ExecuteLoadersAsync());
    }

    [SetUp]
    public void SetUp()
        => ExecutionLog.Clear();
}
