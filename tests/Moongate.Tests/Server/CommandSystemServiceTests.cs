using Moongate.Server.Data.Events;
using Moongate.Server.Services.Console;
using Moongate.Server.Services.Events;
using Moongate.Tests.Server.Support;

namespace Moongate.Tests.Server;

public class CommandSystemServiceTests
{
    [Test]
    public async Task HandleAsync_WhenHelpCommandEntered_ShouldPrintCommandList()
    {
        var gameEventBusService = new GameEventBusService();
        var consoleUiService = new CommandSystemTestConsoleUiService();
        var serverLifetimeService = new CommandSystemTestServerLifetimeService();
        var service = new CommandSystemService(consoleUiService, gameEventBusService, serverLifetimeService);
        await service.StartAsync();

        await gameEventBusService.PublishAsync(new CommandEnteredEvent("help", 1));

        Assert.That(consoleUiService.Lines.Count, Is.GreaterThan(0));
        Assert.That(consoleUiService.Lines[^1].Message, Does.Contain("Available commands:"));
        Assert.That(consoleUiService.Lines[^1].Message, Does.Contain("exit"));
    }

    [Test]
    public async Task HandleAsync_WhenQuestionMarkAliasEntered_ShouldPrintCommandList()
    {
        var gameEventBusService = new GameEventBusService();
        var consoleUiService = new CommandSystemTestConsoleUiService();
        var serverLifetimeService = new CommandSystemTestServerLifetimeService();
        var service = new CommandSystemService(consoleUiService, gameEventBusService, serverLifetimeService);
        await service.StartAsync();

        await gameEventBusService.PublishAsync(new CommandEnteredEvent("?", 1));

        Assert.That(consoleUiService.Lines.Count, Is.GreaterThan(0));
        Assert.That(consoleUiService.Lines[^1].Message, Does.Contain("Available commands:"));
    }

    [Test]
    public async Task HandleAsync_WhenLockCommandEntered_ShouldLockConsoleInput()
    {
        var gameEventBusService = new GameEventBusService();
        var consoleUiService = new CommandSystemTestConsoleUiService();
        var serverLifetimeService = new CommandSystemTestServerLifetimeService();
        var service = new CommandSystemService(consoleUiService, gameEventBusService, serverLifetimeService);
        await service.StartAsync();

        await gameEventBusService.PublishAsync(new CommandEnteredEvent("lock", 1));

        Assert.That(consoleUiService.IsInputLocked, Is.True);
        Assert.That(consoleUiService.Lines[^1].Message, Does.Contain("Console input is locked."));
    }

    [Test]
    public async Task HandleAsync_WhenExitCommandEntered_ShouldRequestShutdown()
    {
        var gameEventBusService = new GameEventBusService();
        var consoleUiService = new CommandSystemTestConsoleUiService();
        var serverLifetimeService = new CommandSystemTestServerLifetimeService();
        var service = new CommandSystemService(consoleUiService, gameEventBusService, serverLifetimeService);
        await service.StartAsync();

        await gameEventBusService.PublishAsync(new CommandEnteredEvent("exit", 1));

        Assert.That(serverLifetimeService.IsShutdownRequested, Is.True);
    }

    [Test]
    public async Task HandleAsync_WhenUnknownCommandEntered_ShouldWriteUnknownCommandMessage()
    {
        var gameEventBusService = new GameEventBusService();
        var consoleUiService = new CommandSystemTestConsoleUiService();
        var serverLifetimeService = new CommandSystemTestServerLifetimeService();
        var service = new CommandSystemService(consoleUiService, gameEventBusService, serverLifetimeService);
        await service.StartAsync();

        await gameEventBusService.PublishAsync(new CommandEnteredEvent("foo", 1));

        Assert.That(consoleUiService.Lines.Count, Is.GreaterThan(0));
        Assert.That(consoleUiService.Lines[^1].Message, Is.EqualTo("Unknown command: foo"));
    }
}
