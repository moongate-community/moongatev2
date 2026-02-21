using System.Net.Sockets;
using Moongate.Network.Client;
using Moongate.Server.Data.Packets;
using Moongate.Server.Data.Session;
using Moongate.Server.Services.Messaging;
using Moongate.Tests.Server.Support;

namespace Moongate.Tests.Server;

public class MessageBusServiceTests
{
    [Test]
    public void PublishIncomingPacket_ShouldBeReadable()
    {
        var bus = new MessageBusService();
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        var session = new GameSession(new(client));
        var packet = new IncomingGamePacket(session, 0xEF, new MessageBusTestPacket(0xEF), 123);

        bus.PublishIncomingPacket(packet);
        Assert.That(bus.CurrentQueueDepth, Is.EqualTo(1));

        var hasPacket = bus.TryReadIncomingPacket(out var readPacket);

        Assert.Multiple(
            () =>
            {
                Assert.That(hasPacket, Is.True);
                Assert.That(readPacket.PacketId, Is.EqualTo(0xEF));
                Assert.That(readPacket.Timestamp, Is.EqualTo(123));
                Assert.That(readPacket.Session.SessionId, Is.EqualTo(session.SessionId));
                Assert.That(bus.CurrentQueueDepth, Is.EqualTo(0));
            }
        );
    }

    [Test]
    public void PublishIncomingPacket_ShouldPreserveFifoOrder()
    {
        var bus = new MessageBusService();
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        var session = new GameSession(new(client));

        bus.PublishIncomingPacket(new(session, 0x01, new MessageBusTestPacket(0x01), 1));
        bus.PublishIncomingPacket(new(session, 0x02, new MessageBusTestPacket(0x02), 2));
        bus.PublishIncomingPacket(new(session, 0x03, new MessageBusTestPacket(0x03), 3));
        Assert.That(bus.CurrentQueueDepth, Is.EqualTo(3));

        var read1 = bus.TryReadIncomingPacket(out var packet1);
        var read2 = bus.TryReadIncomingPacket(out var packet2);
        var read3 = bus.TryReadIncomingPacket(out var packet3);

        Assert.Multiple(
            () =>
            {
                Assert.That(read1 && read2 && read3, Is.True);
                Assert.That(packet1.PacketId, Is.EqualTo(0x01));
                Assert.That(packet2.PacketId, Is.EqualTo(0x02));
                Assert.That(packet3.PacketId, Is.EqualTo(0x03));
                Assert.That(bus.CurrentQueueDepth, Is.EqualTo(0));
            }
        );
    }

    [Test]
    public void TryReadIncomingPacket_WhenEmpty_ShouldReturnFalse()
    {
        var bus = new MessageBusService();

        var hasPacket = bus.TryReadIncomingPacket(out _);

        Assert.That(hasPacket, Is.False);
        Assert.That(bus.CurrentQueueDepth, Is.EqualTo(0));
    }
}
