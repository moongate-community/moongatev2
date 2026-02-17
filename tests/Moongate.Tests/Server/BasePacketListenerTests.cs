using System.Net.Sockets;
using Moongate.Network.Client;
using Moongate.Network.Packets.Incoming.Login;
using Moongate.Network.Packets.Interfaces;
using Moongate.Server.Data.Packets;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Services;
using Moongate.Server.Listeners;
using Moongate.Server.Listeners.Base;

namespace Moongate.Tests.Server;

public class BasePacketListenerTests
{
    private sealed class TestOutgoingPacketQueue : IOutgoingPacketQueue
    {
        private readonly Queue<OutgoingGamePacket> _items = new();

        public void Enqueue(long sessionId, IGameNetworkPacket packet)
            => _items.Enqueue(new OutgoingGamePacket(sessionId, packet, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));

        public bool TryDequeue(out OutgoingGamePacket gamePacket)
        {
            if (_items.Count == 0)
            {
                gamePacket = default;

                return false;
            }

            gamePacket = _items.Dequeue();

            return true;
        }
    }

    private sealed class TestListener : BasePacketListener
    {
        public bool Called { get; private set; }

        public TestListener(IOutgoingPacketQueue outgoingPacketQueue)
            : base(outgoingPacketQueue) { }

        protected override Task<bool> HandleCoreAsync(GameNetworkSession session, IGameNetworkPacket packet)
        {
            Called = true;
            Enqueue(session, packet);

            return Task.FromResult(true);
        }
    }

    [Test]
    public async Task HandlePacketAsync_ShouldDelegateToCoreAndEnqueueOutboundPacket()
    {
        var queue = new TestOutgoingPacketQueue();
        var listener = new TestListener(queue);
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        var session = new GameNetworkSession(client);
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
