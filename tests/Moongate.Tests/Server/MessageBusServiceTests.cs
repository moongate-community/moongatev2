using System.Net.Sockets;
using Moongate.Network.Client;
using Moongate.Network.Packets.Interfaces;
using Moongate.Network.Spans;
using Moongate.Server.Data.Packets;
using Moongate.Server.Data.Session;
using Moongate.Server.Services;

namespace Moongate.Tests.Server;

public class MessageBusServiceTests
{
    private sealed class TestPacket : IGameNetworkPacket
    {
        public TestPacket(byte opCode)
            => OpCode = opCode;

        public byte OpCode { get; }

        public int Length => 1;

        public bool TryParse(ReadOnlySpan<byte> data)
            => true;

        public void Write(ref SpanWriter writer) { }
    }

    [Test]
    public void PublishIncomingPacket_ShouldBeReadable()
    {
        var bus = new MessageBusService();
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        var session = new GameNetworkSession(client);
        var packet = new IncomingGamePacket(session, 0xEF, new TestPacket(0xEF), 123);

        bus.PublishIncomingPacket(packet);

        var hasPacket = bus.TryReadIncomingPacket(out var readPacket);

        Assert.Multiple(
            () =>
            {
                Assert.That(hasPacket, Is.True);
                Assert.That(readPacket.PacketId, Is.EqualTo(0xEF));
                Assert.That(readPacket.Timestamp, Is.EqualTo(123));
                Assert.That(readPacket.Session.SessionId, Is.EqualTo(session.SessionId));
            }
        );
    }

    [Test]
    public void PublishIncomingPacket_ShouldPreserveFifoOrder()
    {
        var bus = new MessageBusService();
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        var session = new GameNetworkSession(client);

        bus.PublishIncomingPacket(new(session, 0x01, new TestPacket(0x01), 1));
        bus.PublishIncomingPacket(new(session, 0x02, new TestPacket(0x02), 2));
        bus.PublishIncomingPacket(new(session, 0x03, new TestPacket(0x03), 3));

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
            }
        );
    }

    [Test]
    public void TryReadIncomingPacket_WhenEmpty_ShouldReturnFalse()
    {
        var bus = new MessageBusService();

        var hasPacket = bus.TryReadIncomingPacket(out _);

        Assert.That(hasPacket, Is.False);
    }
}
