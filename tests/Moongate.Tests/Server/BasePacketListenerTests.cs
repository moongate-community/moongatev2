using System.Net.Sockets;
using Moongate.Network.Client;
using Moongate.Network.Packets.Incoming.Login;
using Moongate.Server.Data.Session;
using Moongate.Tests.Server.Support;

namespace Moongate.Tests.Server;

public class BasePacketListenerTests
{
    [Test]
    public async Task HandlePacketAsync_ShouldDelegateToCoreAndEnqueueOutboundPacket()
    {
        var queue = new BasePacketListenerTestOutgoingPacketQueue();
        var listener = new BasePacketListenerTestListener(queue);
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        var session = new GameSession(new(client));
        var packet = new LoginSeedPacket();

        var handled = await listener.HandlePacketAsync(session, packet);
        var dequeued = queue.TryDequeue(out var outbound);

        Assert.Multiple(
            () =>
            {
                Assert.That(handled, Is.True);
                Assert.That(listener.Called, Is.True);
                Assert.That(dequeued, Is.True);
                Assert.That(outbound.SessionId, Is.EqualTo(session.SessionId));
                Assert.That(outbound.Packet, Is.SameAs(packet));
            }
        );
    }
}
