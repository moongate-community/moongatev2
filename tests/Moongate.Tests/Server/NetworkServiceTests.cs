using System.Net.Sockets;
using System.Reflection;
using Moongate.Core.Types;
using Moongate.Network.Client;
using Moongate.Network.Events;
using Moongate.Network.Packets.Incoming.Login;
using Moongate.Network.Packets.Incoming.Speech;
using Moongate.Server.Data.Events;
using Moongate.Server.Data.Session;
using Moongate.Server.Services.Network;
using Moongate.Server.Services.Packets;
using Moongate.Server.Services.Sessions;
using Moongate.Tests.Server.Support;

namespace Moongate.Tests.Server;

public class NetworkServiceTests
{
    [Test]
    public void OnClientConnected_ShouldPublishPlayerConnectedEvent()
    {
        var messageBus = new NetworkServiceTestMessageBusService();
        var eventBus = new NetworkServiceTestGameEventBusService();
        using var service = new NetworkService(
            messageBus,
            eventBus,
            new PacketDispatchService(),
            new GameNetworkSessionService(),
            new()
            {
                RootDirectory = Path.GetTempPath(),
                LogLevel = LogLevelType.Debug,
                LogPacketData = false
            }
        );
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));

        var method = typeof(NetworkService).GetMethod(
            "OnClientConnected",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        Assert.That(method, Is.Not.Null);
        method!.Invoke(service, [null, new MoongateTCPClientEventArgs(client)]);

        var connectedEvent = eventBus.Events.OfType<PlayerConnectedEvent>().FirstOrDefault();
        Assert.Multiple(
            () =>
            {
                Assert.That(eventBus.Events.Count, Is.GreaterThanOrEqualTo(1));
                Assert.That(connectedEvent.SessionId, Is.EqualTo(client.SessionId));
            }
        );
    }

    [Test]
    public void OnClientData_WhenFixedPacketArrives_ShouldEnqueueTypedGamePacket()
    {
        var messageBus = new NetworkServiceTestMessageBusService();
        var eventBus = new NetworkServiceTestGameEventBusService();
        using var service = new NetworkService(
            messageBus,
            eventBus,
            new PacketDispatchService(),
            new GameNetworkSessionService(),
            new()
            {
                RootDirectory = Path.GetTempPath(),
                LogLevel = LogLevelType.Debug,
                LogPacketData = false
            }
        );
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));

        var payload = new byte[21];
        payload[0] = 0xEF;

        InvokeOnClientData(service, client, payload);

        var dequeued = service.TryDequeueParsedPacket(out var packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(dequeued, Is.True);
                Assert.That(packet.PacketId, Is.EqualTo(0xEF));
                Assert.That(packet.Session.SessionId, Is.EqualTo(client.SessionId));
                Assert.That(packet.Packet, Is.TypeOf<LoginSeedPacket>());
                Assert.That(messageBus.Packets.Count, Is.EqualTo(1));
                Assert.That(messageBus.Packets[0].Packet, Is.TypeOf<LoginSeedPacket>());
            }
        );
    }

    [Test]
    public void OnClientData_WhenMalformedVariablePacketLengthIsZero_ShouldRecoverAndParseNextPacket()
    {
        var messageBus = new NetworkServiceTestMessageBusService();
        var eventBus = new NetworkServiceTestGameEventBusService();
        var sessions = new GameNetworkSessionService();
        using var service = new NetworkService(
            messageBus,
            eventBus,
            new PacketDispatchService(),
            sessions,
            new()
            {
                RootDirectory = Path.GetTempPath(),
                LogLevel = LogLevelType.Debug,
                LogPacketData = false
            }
        );
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));

        var payload = new byte[4 + 3 + 65];
        payload[0] = 0x12;
        payload[1] = 0x34;
        payload[2] = 0x56;
        payload[3] = 0x78;
        payload[4] = 0xAD;
        payload[5] = 0x00;
        payload[6] = 0x00;
        payload[7] = 0x91;

        InvokeOnClientData(service, client, payload);

        var hasPacket = service.TryDequeueParsedPacket(out var parsedPacket);
        var hasAdditionalPackets = service.TryDequeueParsedPacket(out _);

        Assert.Multiple(
            () =>
            {
                Assert.That(hasPacket, Is.True);
                Assert.That(hasAdditionalPackets, Is.False);
                Assert.That(parsedPacket.PacketId, Is.EqualTo(0x91));
                Assert.That(parsedPacket.Packet, Is.TypeOf<GameLoginPacket>());
                Assert.That(messageBus.Packets.Count, Is.EqualTo(1));
                Assert.That(messageBus.Packets[0].PacketId, Is.EqualTo(0x91));
                Assert.That(sessions.TryGet(client.SessionId, out var session), Is.True);
                Assert.That(session.NetworkSession.Seed, Is.EqualTo(0x12345678u));
            }
        );
    }

    [Test]
    public void OnClientData_WhenMultiplePacketsArriveInSingleBuffer_ShouldParseAllInOrder()
    {
        var messageBus = new NetworkServiceTestMessageBusService();
        var eventBus = new NetworkServiceTestGameEventBusService();
        using var service = new NetworkService(
            messageBus,
            eventBus,
            new PacketDispatchService(),
            new GameNetworkSessionService(),
            new()
            {
                RootDirectory = Path.GetTempPath(),
                LogLevel = LogLevelType.Debug,
                LogPacketData = false
            }
        );
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));

        var payload = new byte[21 + 62];
        payload[0] = 0xEF;
        payload[21] = 0x80;

        InvokeOnClientData(service, client, payload);

        var hasFirst = service.TryDequeueParsedPacket(out var firstPacket);
        var hasSecond = service.TryDequeueParsedPacket(out var secondPacket);

        Assert.Multiple(
            () =>
            {
                Assert.That(hasFirst, Is.True);
                Assert.That(hasSecond, Is.True);
                Assert.That(firstPacket.PacketId, Is.EqualTo(0xEF));
                Assert.That(firstPacket.Packet, Is.TypeOf<LoginSeedPacket>());
                Assert.That(secondPacket.PacketId, Is.EqualTo(0x80));
                Assert.That(secondPacket.Packet, Is.TypeOf<AccountLoginPacket>());
                Assert.That(messageBus.Packets.Count, Is.EqualTo(2));
                Assert.That(messageBus.Packets[0].PacketId, Is.EqualTo(0xEF));
                Assert.That(messageBus.Packets[1].PacketId, Is.EqualTo(0x80));
            }
        );
    }

    [Test]
    public void OnClientData_WhenPendingBufferExceedsLimit_ShouldDisconnectSession()
    {
        var messageBus = new NetworkServiceTestMessageBusService();
        var eventBus = new NetworkServiceTestGameEventBusService();
        var sessions = new GameNetworkSessionService();
        using var service = new NetworkService(
            messageBus,
            eventBus,
            new PacketDispatchService(),
            sessions,
            new()
            {
                RootDirectory = Path.GetTempPath(),
                LogLevel = LogLevelType.Debug,
                LogPacketData = false
            }
        );
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));

        var floodPayload = new byte[70_000];
        Array.Fill(floodPayload, (byte)0xAA);

        InvokeOnClientData(service, client, floodPayload);

        Assert.Multiple(
            () =>
            {
                Assert.That(sessions.TryGet(client.SessionId, out var session), Is.True);
                Assert.That(session.NetworkSession.State, Is.EqualTo(NetworkSessionState.Disconnecting));
                Assert.That(service.TryDequeueParsedPacket(out _), Is.False);
                Assert.That(messageBus.Packets.Count, Is.Zero);
            }
        );
    }

    [Test]
    public void OnClientData_WhenProtocolViolationsExceedLimit_ShouldDisconnectSession()
    {
        var messageBus = new NetworkServiceTestMessageBusService();
        var eventBus = new NetworkServiceTestGameEventBusService();
        var sessions = new GameNetworkSessionService();
        using var service = new NetworkService(
            messageBus,
            eventBus,
            new PacketDispatchService(),
            sessions,
            new()
            {
                RootDirectory = Path.GetTempPath(),
                LogLevel = LogLevelType.Debug,
                LogPacketData = false
            }
        );
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));

        var payload = new byte[4 + 40];
        payload[0] = 0x11;
        payload[1] = 0x22;
        payload[2] = 0x33;
        payload[3] = 0x44;
        Array.Fill(payload, (byte)0xFF, 4, 40);

        InvokeOnClientData(service, client, payload);

        Assert.Multiple(
            () =>
            {
                Assert.That(sessions.TryGet(client.SessionId, out var session), Is.True);
                Assert.That(session.NetworkSession.State, Is.EqualTo(NetworkSessionState.Disconnecting));
                Assert.That(service.TryDequeueParsedPacket(out _), Is.False);
                Assert.That(messageBus.Packets.Count, Is.Zero);
            }
        );
    }

    [Test]
    public void OnClientData_WhenUnknownOpcodesStayBelowLimit_ShouldContinueParsingSubsequentValidPacket()
    {
        var messageBus = new NetworkServiceTestMessageBusService();
        var eventBus = new NetworkServiceTestGameEventBusService();
        var sessions = new GameNetworkSessionService();
        using var service = new NetworkService(
            messageBus,
            eventBus,
            new PacketDispatchService(),
            sessions,
            new()
            {
                RootDirectory = Path.GetTempPath(),
                LogLevel = LogLevelType.Debug,
                LogPacketData = false
            }
        );
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));

        InvokeOnClientData(service, client, [0x12, 0x34, 0x56, 0x78]);

        var payload = new byte[31 + 21];
        Array.Fill(payload, (byte)0xFF, 0, 31);
        payload[31] = 0xEF;

        InvokeOnClientData(service, client, payload);

        var hasParsedPacket = service.TryDequeueParsedPacket(out var parsedPacket);

        Assert.Multiple(
            () =>
            {
                Assert.That(hasParsedPacket, Is.True);
                Assert.That(parsedPacket.PacketId, Is.EqualTo(0xEF));
                Assert.That(parsedPacket.Packet, Is.TypeOf<LoginSeedPacket>());
                Assert.That(sessions.TryGet(client.SessionId, out var session), Is.True);
                Assert.That(session.NetworkSession.State, Is.EqualTo(NetworkSessionState.Login));
                Assert.That(messageBus.Packets.Count, Is.EqualTo(1));
            }
        );
    }

    [Test]
    public void OnClientData_WhenVariablePacketLengthExceedsLimit_ShouldDropMalformedHeaderAndParseFollowingPacket()
    {
        var messageBus = new NetworkServiceTestMessageBusService();
        var eventBus = new NetworkServiceTestGameEventBusService();
        using var service = new NetworkService(
            messageBus,
            eventBus,
            new PacketDispatchService(),
            new GameNetworkSessionService(),
            new()
            {
                RootDirectory = Path.GetTempPath(),
                LogLevel = LogLevelType.Debug,
                LogPacketData = false
            }
        );
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));

        var payload = new byte[4 + 3 + 62];
        payload[0] = 0x12;
        payload[1] = 0x34;
        payload[2] = 0x56;
        payload[3] = 0x78;
        payload[4] = 0xAD;
        payload[5] = 0x40;
        payload[6] = 0x01;
        payload[7] = 0x80;

        InvokeOnClientData(service, client, payload);

        var hasPacket = service.TryDequeueParsedPacket(out var parsedPacket);
        var hasOtherPackets = service.TryDequeueParsedPacket(out _);

        Assert.Multiple(
            () =>
            {
                Assert.That(hasPacket, Is.True);
                Assert.That(hasOtherPackets, Is.False);
                Assert.That(parsedPacket.PacketId, Is.EqualTo(0x80));
                Assert.That(parsedPacket.Packet, Is.TypeOf<AccountLoginPacket>());
                Assert.That(messageBus.Packets.Count, Is.EqualTo(1));
                Assert.That(messageBus.Packets[0].PacketId, Is.EqualTo(0x80));
            }
        );
    }

    [Test]
    public void OnClientData_WhenReconnectSeedAndGameLoginAreInSameBuffer_ShouldParseGameLoginPacket()
    {
        var messageBus = new NetworkServiceTestMessageBusService();
        var eventBus = new NetworkServiceTestGameEventBusService();
        var sessions = new GameNetworkSessionService();
        using var service = new NetworkService(
            messageBus,
            eventBus,
            new PacketDispatchService(),
            sessions,
            new()
            {
                RootDirectory = Path.GetTempPath(),
                LogLevel = LogLevelType.Debug,
                LogPacketData = false
            }
        );
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));

        var payload = new byte[69];
        payload[0] = 0x11;
        payload[1] = 0x22;
        payload[2] = 0x33;
        payload[3] = 0x44;
        payload[4] = 0x91;

        InvokeOnClientData(service, client, payload);

        var dequeued = service.TryDequeueParsedPacket(out var packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(dequeued, Is.True);
                Assert.That(packet.PacketId, Is.EqualTo(0x91));
                Assert.That(packet.Packet, Is.TypeOf<GameLoginPacket>());
                Assert.That(sessions.TryGet(client.SessionId, out var session), Is.True);
                Assert.That(session.NetworkSession.Seed, Is.EqualTo(0x11223344u));
            }
        );
    }

    [Test]
    public void OnClientData_WhenReconnectSeedIsZero_ShouldDiscardAndWaitForNewHandshakeData()
    {
        var messageBus = new NetworkServiceTestMessageBusService();
        var eventBus = new NetworkServiceTestGameEventBusService();
        var sessions = new GameNetworkSessionService();
        using var service = new NetworkService(
            messageBus,
            eventBus,
            new PacketDispatchService(),
            sessions,
            new()
            {
                RootDirectory = Path.GetTempPath(),
                LogLevel = LogLevelType.Debug,
                LogPacketData = false
            }
        );
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));

        InvokeOnClientData(service, client, [0x00, 0x00, 0x00, 0x00]);
        Assert.That(service.TryDequeueParsedPacket(out _), Is.False);

        var loginSeedPacket = new byte[21];
        loginSeedPacket[0] = 0xEF;
        InvokeOnClientData(service, client, loginSeedPacket);
        var hasPacket = service.TryDequeueParsedPacket(out var packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(hasPacket, Is.True);
                Assert.That(packet.PacketId, Is.EqualTo(0xEF));
                Assert.That(packet.Packet, Is.TypeOf<LoginSeedPacket>());
                Assert.That(sessions.TryGet(client.SessionId, out var session), Is.True);
                Assert.That(session.NetworkSession.State, Is.EqualTo(NetworkSessionState.Login));
                Assert.That(messageBus.Packets.Count, Is.EqualTo(1));
            }
        );
    }

    [Test]
    public void OnClientData_WhenVariablePacketIsFragmented_ShouldParseAfterLengthIsComplete()
    {
        var messageBus = new NetworkServiceTestMessageBusService();
        var eventBus = new NetworkServiceTestGameEventBusService();
        var sessions = new GameNetworkSessionService();
        using var service = new NetworkService(
            messageBus,
            eventBus,
            new PacketDispatchService(),
            sessions,
            new()
            {
                RootDirectory = Path.GetTempPath(),
                LogLevel = LogLevelType.Debug,
                LogPacketData = false
            }
        );
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));

        // Seed can arrive fragmented on reconnect (4 bytes).
        InvokeOnClientData(service, client, [0x12, 0x34]);
        Assert.That(service.TryDequeueParsedPacket(out _), Is.False);

        InvokeOnClientData(service, client, [0x56, 0x78]);
        Assert.That(service.TryDequeueParsedPacket(out _), Is.False);

        InvokeOnClientData(service, client, [0xAD, 0x00]);

        Assert.That(service.TryDequeueParsedPacket(out _), Is.False);

        InvokeOnClientData(service, client, [0x03]);

        var dequeued = service.TryDequeueParsedPacket(out var packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(dequeued, Is.True);
                Assert.That(packet.PacketId, Is.EqualTo(0xAD));
                Assert.That(packet.Packet, Is.TypeOf<UnicodeSpeechPacket>());
                Assert.That(messageBus.Packets.Count, Is.EqualTo(1));
                Assert.That(messageBus.Packets[0].Packet, Is.TypeOf<UnicodeSpeechPacket>());
                Assert.That(sessions.TryGet(client.SessionId, out var session), Is.True);
                Assert.That(session.NetworkSession.Seed, Is.EqualTo(0x12345678u));
            }
        );
    }

    private static void InvokeOnClientData(NetworkService service, MoongateTCPClient client, byte[] payload)
    {
        var method = typeof(NetworkService).GetMethod(
            "OnClientData",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        Assert.That(method, Is.Not.Null);
        method!.Invoke(service, [null, new MoongateTCPDataReceivedEventArgs(client, payload)]);
    }
}
