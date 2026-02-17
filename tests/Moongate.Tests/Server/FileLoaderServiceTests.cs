using DryIoc;
using Moongate.Server.Services;
using Moongate.UO.Data.Interfaces.FileLoaders;

namespace Moongate.Tests.Server;

public class FileLoaderServiceTests
{
    [SetUp]
    public void SetUp()
        => ExecutionLog.Clear();

    [Test]
    public async Task AddFileLoader_WhenCalledTwiceForSameType_ShouldExecuteOnlyOnce()
    {
        using var container = new Container();
        var service = new FileLoaderService(container);

        service.AddFileLoader<LoaderA>();
        service.AddFileLoader<LoaderA>();

        await service.ExecuteLoadersAsync();

        Assert.That(ExecutionLog, Is.EqualTo(["A"]));
    }

    [Test]
    public void AddFileLoader_WhenTypeIsNotRegistered_ShouldRegisterItInContainer()
    {
        using var container = new Container();
        var service = new FileLoaderService(container);

        Assert.That(container.IsRegistered<LoaderA>(), Is.False);

        service.AddFileLoader<LoaderA>();

        Assert.That(container.IsRegistered<LoaderA>(), Is.True);
    }

    [Test]
    public async Task ExecuteLoadersAsync_ShouldRunLoadersInInsertionOrder()
    {
        using var container = new Container();
        var service = new FileLoaderService(container);

        service.AddFileLoader<LoaderA>();
        service.AddFileLoader<LoaderB>();

        await service.ExecuteLoadersAsync();

        Assert.That(ExecutionLog, Is.EqualTo(["A", "B"]));
    }

    [Test]
    public void ExecuteLoadersAsync_WhenLoaderThrows_ShouldPropagateException()
    {
        using var container = new Container();
        var service = new FileLoaderService(container);

        service.AddFileLoader<LoaderThrows>();

        Assert.ThrowsAsync<InvalidOperationException>(async () => await service.ExecuteLoadersAsync());
    }

    private static List<string> ExecutionLog { get; } = [];

    private sealed class LoaderA : IFileLoader
    {
        public Task LoadAsync()
        {
            ExecutionLog.Add("A");
            return Task.CompletedTask;
        }
    }

    private sealed class LoaderB : IFileLoader
    {
        public Task LoadAsync()
        {
            ExecutionLog.Add("B");
            return Task.CompletedTask;
        }
    }

    private sealed class LoaderThrows : IFileLoader
    {
        public Task LoadAsync()
            => throw new InvalidOperationException("boom");
    }
}
