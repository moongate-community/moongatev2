using System.Net.Sockets;
using Moongate.Network.Client;
using Moongate.Server.Data.Session;
using Moongate.Server.Services.GameLoop;
using Moongate.Server.Services.Messaging;
using Moongate.Server.Services.Packets;
using Moongate.Server.Services.Sessions;
using Moongate.Server.Services.Timing;
using Moongate.Tests.Server.Support;

namespace Moongate.Tests.Server;

public class GameLoopServiceTests
{
    private static readonly int[] ExpectedDispatchSequence = [1, 2, 3];
    private GameLoopService? _service;

    [Test]
    public void Ctor_ShouldStartWithZeroedMetrics()
    {
        _service = new(
            new PacketDispatchService(),
            new MessageBusService(),
            new OutgoingPacketQueue(),
            new GameNetworkSessionService(),
            CreateTimerService(),
            new GameLoopTestOutboundPacketSender()
        );

        Assert.Multiple(
            () =>
            {
                var snapshot = _service.GetMetricsSnapshot();
                Assert.That(snapshot.TickCount, Is.Zero);
                Assert.That(snapshot.Uptime, Is.EqualTo(TimeSpan.Zero));
                Assert.That(snapshot.AverageTickMs, Is.Zero);
            }
        );
    }

    [Test]
    public async Task StartAsync_ShouldAdvanceLoopAndUpdateMetrics()
    {
        _service = new(
            new PacketDispatchService(),
            new MessageBusService(),
            new OutgoingPacketQueue(),
            new GameNetworkSessionService(),
            CreateTimerService(),
            new GameLoopTestOutboundPacketSender()
        );

        await _service.StartAsync();

        var tickAdvanced = await WaitUntilAsync(() => _service.GetMetricsSnapshot().TickCount > 0, TimeSpan.FromSeconds(2));
        var uptimeAdvanced = await WaitUntilAsync(
                                 () => _service.GetMetricsSnapshot().Uptime > TimeSpan.Zero,
                                 TimeSpan.FromSeconds(2)
                             );

        Assert.Multiple(
            () =>
            {
                Assert.That(tickAdvanced, Is.True, "TickCount did not increase in time.");
                Assert.That(uptimeAdvanced, Is.True, "Uptime did not increase in time.");
                Assert.That(_service.GetMetricsSnapshot().AverageTickMs, Is.GreaterThanOrEqualTo(0d));
            }
        );
    }

    [Test]
    public async Task StartAsync_ShouldDrainMessageBusAndDispatchPacketsInOrder()
    {
        var messageBus = new MessageBusService();
        var packetDispatch = new PacketDispatchService();
        var listener = new GameLoopRecordingPacketListener();
        packetDispatch.AddPacketListener(0xAA, listener);

        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        var session = new GameSession(new(client));
        messageBus.PublishIncomingPacket(new(session, 0xAA, new GameLoopTestPacket(0xAA, 1), 1));
        messageBus.PublishIncomingPacket(new(session, 0xAA, new GameLoopTestPacket(0xAA, 2), 2));
        messageBus.PublishIncomingPacket(new(session, 0xAA, new GameLoopTestPacket(0xAA, 3), 3));

        _service = new(
            packetDispatch,
            messageBus,
            new OutgoingPacketQueue(),
            new GameNetworkSessionService(),
            CreateTimerService(),
            new GameLoopTestOutboundPacketSender()
        );

        await _service.StartAsync();
        var drained = await WaitUntilAsync(() => listener.Sequences.Count == 3, TimeSpan.FromSeconds(2));

        Assert.Multiple(
            () =>
            {
                Assert.That(drained, Is.True, "Packet queue was not drained in time.");
                Assert.That(listener.Sequences, Is.EqualTo(ExpectedDispatchSequence));
            }
        );
    }

    [Test]
    public async Task StartAsync_ShouldDrainOutgoingPacketQueueAndSendPackets()
    {
        var sessions = new GameNetworkSessionService();
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        var session = sessions.GetOrCreate(client);
        var outgoingQueue = new OutgoingPacketQueue();
        var sender = new GameLoopTestOutboundPacketSender();
        outgoingQueue.Enqueue(session.SessionId, new GameLoopTestPacket(0x20, 0));

        _service = new(
            new PacketDispatchService(),
            new MessageBusService(),
            outgoingQueue,
            sessions,
            CreateTimerService(),
            sender
        );

        await _service.StartAsync();
        var sent = await WaitUntilAsync(() => sender.SentPackets.Count == 1, TimeSpan.FromSeconds(2));

        Assert.Multiple(
            () =>
            {
                Assert.That(sent, Is.True, "Outbound packet was not sent in time.");
                Assert.That(sender.SentPackets[0].SessionId, Is.EqualTo(session.SessionId));
                Assert.That(sender.SentPackets[0].Packet.OpCode, Is.EqualTo(0x20));
            }
        );
    }

    [Test]
    public async Task StartAsync_ShouldProcessTimerWheelInProcessQueue()
    {
        var timerService = new TimerWheelService(
            new()
            {
                TickDuration = TimeSpan.FromMilliseconds(250),
                WheelSize = 512
            }
        );
        var fired = 0;

        timerService.RegisterTimer("test", TimeSpan.FromMilliseconds(250), () => Interlocked.Increment(ref fired));

        _service = new(
            new PacketDispatchService(),
            new MessageBusService(),
            new OutgoingPacketQueue(),
            new GameNetworkSessionService(),
            timerService,
            new GameLoopTestOutboundPacketSender()
        );

        await _service.StartAsync();

        var timerFired = await WaitUntilAsync(() => Volatile.Read(ref fired) > 0, TimeSpan.FromSeconds(2));

        Assert.That(timerFired, Is.True, "Timer callback was not executed by game loop processing.");
    }

    [Test]
    public async Task StartAsync_WhenListenerThrows_ShouldStillInvokeOtherListeners()
    {
        var messageBus = new MessageBusService();
        var packetDispatch = new PacketDispatchService();
        var successfulListener = new GameLoopRecordingPacketListener();
        packetDispatch.AddPacketListener(0xAB, new GameLoopThrowingPacketListener());
        packetDispatch.AddPacketListener(0xAB, successfulListener);

        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        var session = new GameSession(new(client));
        messageBus.PublishIncomingPacket(new(session, 0xAB, new GameLoopTestPacket(0xAB, 42), 1));

        _service = new(
            packetDispatch,
            messageBus,
            new OutgoingPacketQueue(),
            new GameNetworkSessionService(),
            CreateTimerService(),
            new GameLoopTestOutboundPacketSender()
        );

        await _service.StartAsync();
        var invoked = await WaitUntilAsync(() => successfulListener.Sequences.Count == 1, TimeSpan.FromSeconds(2));

        Assert.Multiple(
            () =>
            {
                Assert.That(invoked, Is.True, "Non-throwing listener was not invoked.");
                Assert.That(successfulListener.Sequences.Single(), Is.EqualTo(42));
            }
        );
    }

    [Test]
    public async Task StopAsync_ShouldStopAdvancingTickCount()
    {
        _service = new(
            new PacketDispatchService(),
            new MessageBusService(),
            new OutgoingPacketQueue(),
            new GameNetworkSessionService(),
            CreateTimerService(),
            new GameLoopTestOutboundPacketSender()
        );
        await _service.StartAsync();

        var tickAdvanced = await WaitUntilAsync(
                               () => _service.GetMetricsSnapshot().TickCount > 0,
                               TimeSpan.FromSeconds(2)
                           );
        Assert.That(tickAdvanced, Is.True, "TickCount did not increase in time.");

        await _service.StopAsync();

        var tickAfterStop = _service.GetMetricsSnapshot().TickCount;
        await Task.Delay(500);

        Assert.That(_service.GetMetricsSnapshot().TickCount, Is.LessThanOrEqualTo(tickAfterStop + 1));
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_service is not null)
        {
            await _service.StopAsync();
            _service.Dispose();
        }

        _service = null;
    }

    private static TimerWheelService CreateTimerService()
        => new(
            new()
            {
                TickDuration = TimeSpan.FromMilliseconds(150),
                WheelSize = 512
            }
        );

    private static async Task<bool> WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
    {
        var start = DateTime.UtcNow;

        while (DateTime.UtcNow - start < timeout)
        {
            if (condition())
            {
                return true;
            }

            await Task.Delay(20);
        }

        return condition();
    }
}
