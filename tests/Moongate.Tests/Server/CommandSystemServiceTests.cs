using Moongate.Server.Data.Events;
using Moongate.Server.Data.Session;
using Moongate.Server.Services.Console;
using Moongate.Server.Services.Events;
using Moongate.Server.Types.Commands;
using Moongate.Tests.Server.Support;
using Moongate.Network.Client;
using Moongate.Network.Packets.Outgoing.Speech;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Types;
using System.Net.Sockets;

namespace Moongate.Tests.Server;

public class CommandSystemServiceTests
{
    [Test]
    public async Task HandleAsync_WhenExitCommandEntered_ShouldRequestShutdown()
    {
        var gameEventBusService = new GameEventBusService();
        var consoleUiService = new CommandSystemTestConsoleUiService();
        var outgoingPacketQueue = new BasePacketListenerTestOutgoingPacketQueue();
        var serverLifetimeService = new CommandSystemTestServerLifetimeService();
        var service = new CommandSystemService(
            consoleUiService,
            gameEventBusService,
            outgoingPacketQueue,
            serverLifetimeService
        );
        await service.StartAsync();

        await gameEventBusService.PublishAsync(new CommandEnteredEvent("exit", CommandSourceType.Console));

        Assert.That(serverLifetimeService.IsShutdownRequested, Is.True);
    }

    [Test]
    public async Task HandleAsync_WhenHelpCommandEntered_ShouldPrintCommandList()
    {
        var gameEventBusService = new GameEventBusService();
        var consoleUiService = new CommandSystemTestConsoleUiService();
        var outgoingPacketQueue = new BasePacketListenerTestOutgoingPacketQueue();
        var serverLifetimeService = new CommandSystemTestServerLifetimeService();
        var service = new CommandSystemService(
            consoleUiService,
            gameEventBusService,
            outgoingPacketQueue,
            serverLifetimeService
        );
        await service.StartAsync();

        await gameEventBusService.PublishAsync(new CommandEnteredEvent("help", CommandSourceType.Console));

        Assert.That(consoleUiService.Lines.Count, Is.GreaterThan(0));
        Assert.That(consoleUiService.Lines[^1].Message, Does.Contain("Available commands:"));
        Assert.That(consoleUiService.Lines[^1].Message, Does.Contain("exit"));
    }

    [Test]
    public async Task HandleAsync_WhenLockCommandEntered_ShouldLockConsoleInput()
    {
        var gameEventBusService = new GameEventBusService();
        var consoleUiService = new CommandSystemTestConsoleUiService();
        var outgoingPacketQueue = new BasePacketListenerTestOutgoingPacketQueue();
        var serverLifetimeService = new CommandSystemTestServerLifetimeService();
        var service = new CommandSystemService(
            consoleUiService,
            gameEventBusService,
            outgoingPacketQueue,
            serverLifetimeService
        );
        await service.StartAsync();

        await gameEventBusService.PublishAsync(new CommandEnteredEvent("lock", CommandSourceType.Console));

        Assert.That(consoleUiService.IsInputLocked, Is.True);
        Assert.That(consoleUiService.Lines[^1].Message, Does.Contain("Console input is locked."));
    }

    [Test]
    public async Task HandleAsync_WhenQuestionMarkAliasEntered_ShouldPrintCommandList()
    {
        var gameEventBusService = new GameEventBusService();
        var consoleUiService = new CommandSystemTestConsoleUiService();
        var outgoingPacketQueue = new BasePacketListenerTestOutgoingPacketQueue();
        var serverLifetimeService = new CommandSystemTestServerLifetimeService();
        var service = new CommandSystemService(
            consoleUiService,
            gameEventBusService,
            outgoingPacketQueue,
            serverLifetimeService
        );
        await service.StartAsync();

        await gameEventBusService.PublishAsync(new CommandEnteredEvent("?", CommandSourceType.Console));

        Assert.That(consoleUiService.Lines.Count, Is.GreaterThan(0));
        Assert.That(consoleUiService.Lines[^1].Message, Does.Contain("Available commands:"));
    }

    [Test]
    public async Task HandleAsync_WhenUnknownCommandEntered_ShouldWriteUnknownCommandMessage()
    {
        var gameEventBusService = new GameEventBusService();
        var consoleUiService = new CommandSystemTestConsoleUiService();
        var outgoingPacketQueue = new BasePacketListenerTestOutgoingPacketQueue();
        var serverLifetimeService = new CommandSystemTestServerLifetimeService();
        var service = new CommandSystemService(
            consoleUiService,
            gameEventBusService,
            outgoingPacketQueue,
            serverLifetimeService
        );
        await service.StartAsync();

        await gameEventBusService.PublishAsync(new CommandEnteredEvent("foo", CommandSourceType.Console));

        Assert.That(consoleUiService.Lines.Count, Is.GreaterThan(0));
        Assert.That(consoleUiService.Lines[^1].Message, Is.EqualTo("Unknown command: foo"));
    }

    [Test]
    public async Task ExecuteCommandAsync_WhenSourceIsInGame_ShouldSendSpeechPacketsPerOutputLine()
    {
        var gameEventBusService = new GameEventBusService();
        var consoleUiService = new CommandSystemTestConsoleUiService();
        var outgoingPacketQueue = new BasePacketListenerTestOutgoingPacketQueue();
        var serverLifetimeService = new CommandSystemTestServerLifetimeService();
        var service = new CommandSystemService(
            consoleUiService,
            gameEventBusService,
            outgoingPacketQueue,
            serverLifetimeService
        );

        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        var session = new GameSession(new(client))
        {
            Character = new UOMobileEntity
            {
                Id = (Serial)0x00000010,
                Name = "TestCharacter",
                BaseBody = 0x0190
            }
        };

        await service.ExecuteCommandAsync("help", CommandSourceType.InGame, session);

        var messages = new List<string>();
        while (outgoingPacketQueue.TryDequeue(out var outbound))
        {
            if (outbound.Packet is UnicodeSpeechMessagePacket speechPacket)
            {
                messages.Add(speechPacket.Text);
            }
        }

        Assert.Multiple(
            () =>
            {
                Assert.That(messages.Count, Is.GreaterThan(0));
                Assert.That(messages[0], Is.EqualTo("Available commands:"));
                Assert.That(messages.Any(message => message.Contains("help")), Is.True);
                Assert.That(consoleUiService.Lines.Count, Is.EqualTo(0));
            }
        );
    }
}
