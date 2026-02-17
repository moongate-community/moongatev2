using System.Net.Sockets;
using System.Reflection;
using Moongate.Network.Client;
using Moongate.Network.Events;
using Moongate.Network.Packets.Incoming.Login;
using Moongate.Network.Packets.Incoming.Speech;
using Moongate.Core.Types;
using Moongate.Server.Data.Config;
using Moongate.Server.Data.Packets;
using Moongate.Server.Interfaces.Services;
using Moongate.Server.Services;

namespace Moongate.Tests.Server;

public class NetworkServiceTests
{
    private sealed class TestGamePacketIngress : IGamePacketIngress
    {
        private readonly List<GamePacket> _packets = [];

        public List<GamePacket> Packets => _packets;

        public void EnqueueGamePacket(GamePacket gamePacket)
            => _packets.Add(gamePacket);
    }

    [Test]
    public void OnClientData_WhenFixedPacketArrives_ShouldEnqueueTypedGamePacket()
    {
        var ingress = new TestGamePacketIngress();
        using var service = new NetworkService(
            ingress,
            new PacketDispatchService(),
            new GameNetworkSessionService(),
            new MoongateConfig
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
                Assert.That(ingress.Packets.Count, Is.EqualTo(1));
                Assert.That(ingress.Packets[0].Packet, Is.TypeOf<LoginSeedPacket>());
            }
        );
    }

    [Test]
    public void OnClientData_WhenVariablePacketIsFragmented_ShouldParseAfterLengthIsComplete()
    {
        var ingress = new TestGamePacketIngress();
        using var service = new NetworkService(
            ingress,
            new PacketDispatchService(),
            new GameNetworkSessionService(),
            new MoongateConfig
            {
                RootDirectory = Path.GetTempPath(),
                LogLevel = LogLevelType.Debug,
                LogPacketData = false
            }
        );
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));

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
                Assert.That(ingress.Packets.Count, Is.EqualTo(1));
                Assert.That(ingress.Packets[0].Packet, Is.TypeOf<UnicodeSpeechPacket>());
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
