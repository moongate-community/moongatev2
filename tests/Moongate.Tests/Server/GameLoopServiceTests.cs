using System.Net.Sockets;
using Moongate.Server.Services;
using Moongate.Network.Client;
using Moongate.Network.Packets.Interfaces;
using Moongate.Network.Spans;
using Moongate.Server.Data.Packets;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Listener;
using Moongate.Server.Interfaces.Services;

namespace Moongate.Tests.Server;

public class GameLoopServiceTests
{
    private GameLoopService? _service;

    [Test]
    public void Ctor_ShouldStartWithZeroedMetrics()
    {
        _service = new(
            new PacketDispatchService(),
            new MessageBusService(),
            new OutgoingPacketQueue(),
            new GameNetworkSessionService(),
            new TimerWheelService(),
            new TestOutboundPacketSender()
        );

        Assert.Multiple(
            () =>
            {
                Assert.That(_service.TickCount, Is.Zero);
                Assert.That(_service.Uptime, Is.EqualTo(TimeSpan.Zero));
                Assert.That(_service.AverageTickMs, Is.Zero);
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
            new TimerWheelService(),
            new TestOutboundPacketSender()
        );

        await _service.StartAsync();

        var tickAdvanced = await WaitUntilAsync(() => _service.TickCount > 0, TimeSpan.FromSeconds(2));
        var uptimeAdvanced = await WaitUntilAsync(() => _service.Uptime > TimeSpan.Zero, TimeSpan.FromSeconds(2));

        Assert.Multiple(
            () =>
            {
                Assert.That(tickAdvanced, Is.True, "TickCount did not increase in time.");
                Assert.That(uptimeAdvanced, Is.True, "Uptime did not increase in time.");
                Assert.That(_service.AverageTickMs, Is.GreaterThanOrEqualTo(0d));
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
            new TimerWheelService(),
            new TestOutboundPacketSender()
        );
        await _service.StartAsync();

        var tickAdvanced = await WaitUntilAsync(() => _service.TickCount > 0, TimeSpan.FromSeconds(2));
        Assert.That(tickAdvanced, Is.True, "TickCount did not increase in time.");

        await _service.StopAsync();

        var tickAfterStop = _service.TickCount;
        await Task.Delay(500);

        Assert.That(_service.TickCount, Is.LessThanOrEqualTo(tickAfterStop + 1));
    }

    [Test]
    public async Task StartAsync_ShouldProcessTimerWheelInProcessQueue()
    {
        var timerService = new TimerWheelService(TimeSpan.FromMilliseconds(250));
        var fired = 0;

        timerService.RegisterTimer("test", TimeSpan.FromMilliseconds(250), () => Interlocked.Increment(ref fired));

        _service = new(
            new PacketDispatchService(),
            new MessageBusService(),
            new OutgoingPacketQueue(),
            new GameNetworkSessionService(),
            timerService,
            new TestOutboundPacketSender()
        );

        await _service.StartAsync();

        var timerFired = await WaitUntilAsync(() => Volatile.Read(ref fired) > 0, TimeSpan.FromSeconds(2));

        Assert.That(timerFired, Is.True, "Timer callback was not executed by game loop processing.");
    }

    [Test]
    public async Task StartAsync_ShouldDrainMessageBusAndDispatchPacketsInOrder()
    {
        var messageBus = new MessageBusService();
        var packetDispatch = new PacketDispatchService();
        var listener = new RecordingPacketListener();
        packetDispatch.AddPacketListener(0xAA, listener);

        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        var session = new GameSession(new GameNetworkSession(client));
        messageBus.PublishIncomingPacket(new(session, 0xAA, new TestPacket(0xAA, 1), 1));
        messageBus.PublishIncomingPacket(new(session, 0xAA, new TestPacket(0xAA, 2), 2));
        messageBus.PublishIncomingPacket(new(session, 0xAA, new TestPacket(0xAA, 3), 3));

        _service = new(
            packetDispatch,
            messageBus,
            new OutgoingPacketQueue(),
            new GameNetworkSessionService(),
            new TimerWheelService(),
            new TestOutboundPacketSender()
        );

        await _service.StartAsync();
        var drained = await WaitUntilAsync(() => listener.Sequences.Count == 3, TimeSpan.FromSeconds(2));

        Assert.Multiple(
            () =>
            {
                Assert.That(drained, Is.True, "Packet queue was not drained in time.");
                Assert.That(listener.Sequences, Is.EqualTo(new[] { 1, 2, 3 }));
            }
        );
    }

    [Test]
    public async Task StartAsync_WhenListenerThrows_ShouldStillInvokeOtherListeners()
    {
        var messageBus = new MessageBusService();
        var packetDispatch = new PacketDispatchService();
        var successfulListener = new RecordingPacketListener();
        packetDispatch.AddPacketListener(0xAB, new ThrowingPacketListener());
        packetDispatch.AddPacketListener(0xAB, successfulListener);

        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        var session = new GameSession(new GameNetworkSession(client));
        messageBus.PublishIncomingPacket(new(session, 0xAB, new TestPacket(0xAB, 42), 1));

        _service = new(
            packetDispatch,
            messageBus,
            new OutgoingPacketQueue(),
            new GameNetworkSessionService(),
            new TimerWheelService(),
            new TestOutboundPacketSender()
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
    public async Task StartAsync_ShouldDrainOutgoingPacketQueueAndSendPackets()
    {
        var sessions = new GameNetworkSessionService();
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        var session = sessions.GetOrCreate(client);
        var outgoingQueue = new OutgoingPacketQueue();
        var sender = new TestOutboundPacketSender();
        outgoingQueue.Enqueue(session.SessionId, new TestPacket(0x20, 0));

        _service = new(
            new PacketDispatchService(),
            new MessageBusService(),
            outgoingQueue,
            sessions,
            new TimerWheelService(),
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

    private sealed class TestPacket : IGameNetworkPacket
    {
        public TestPacket(byte opCode, int sequence)
        {
            OpCode = opCode;
            Sequence = sequence;
        }

        public int Sequence { get; }
        public byte OpCode { get; }
        public int Length => 1;

        public bool TryParse(ReadOnlySpan<byte> data)
            => true;

        public void Write(ref SpanWriter writer) { }
    }

    private sealed class RecordingPacketListener : IPacketListener
    {
        public List<int> Sequences { get; } = [];

        public Task<bool> HandlePacketAsync(GameSession session, IGameNetworkPacket packet)
        {
            if (packet is TestPacket testPacket)
            {
                Sequences.Add(testPacket.Sequence);
            }

            return Task.FromResult(true);
        }
    }

    private sealed class ThrowingPacketListener : IPacketListener
    {
        public Task<bool> HandlePacketAsync(GameSession session, IGameNetworkPacket packet)
            => throw new InvalidOperationException("listener failure");
    }

    private sealed class TestOutboundPacketSender : IOutboundPacketSender
    {
        public List<OutgoingGamePacket> SentPackets { get; } = [];

        public Task<bool> SendAsync(
            MoongateTCPClient client,
            OutgoingGamePacket outgoingPacket,
            CancellationToken cancellationToken
        )
        {
            SentPackets.Add(outgoingPacket);
            return Task.FromResult(true);
        }
    }
}
