using System.Net.Sockets;
using Moongate.Network.Client;
using Moongate.Network.Packets.Incoming.Speech;
using Moongate.Network.Packets.Outgoing.Speech;
using Moongate.Server.Data.Session;
using Moongate.Server.Handlers;
using Moongate.Tests.Server.Support;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.Server;

public class SpeechHandlerTests
{
    [Test]
    public async Task HandlePacketAsync_ShouldEnqueueUnicodeSpeechMessagePacket_ForSenderSession()
    {
        var queue = new BasePacketListenerTestOutgoingPacketQueue();
        var handler = new SpeechHandler(queue, new MockCommandSystemService());
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));

        var session = new GameSession(new(client))
        {
            Character = new UOMobileEntity
            {
                Id = (Serial)0x00000002,
                Name = "Tom",
                BaseBody = 0x0190
            }
        };

        var packet = new UnicodeSpeechPacket
        {
            MessageType = ChatMessageType.Regular,
            Hue = 0x0035,
            Font = 0x0003,
            Language = "ENU",
            Text = "hello"
        };

        var handled = await handler.HandlePacketAsync(session, packet);
        var dequeued = queue.TryDequeue(out var outbound);

        Assert.Multiple(
            () =>
            {
                Assert.That(handled, Is.True);
                Assert.That(dequeued, Is.True);
                Assert.That(outbound.SessionId, Is.EqualTo(session.SessionId));
                Assert.That(outbound.Packet, Is.TypeOf<UnicodeSpeechMessagePacket>());
            }
        );

        var speechMessagePacket = (UnicodeSpeechMessagePacket)outbound.Packet;

        Assert.Multiple(
            () =>
            {
                Assert.That(speechMessagePacket.Serial, Is.EqualTo((Serial)0x00000002));
                Assert.That(speechMessagePacket.MessageType, Is.EqualTo(ChatMessageType.Regular));
                Assert.That(speechMessagePacket.Hue, Is.EqualTo(0x0035));
                Assert.That(speechMessagePacket.Font, Is.EqualTo(0x0003));
                Assert.That(speechMessagePacket.Language, Is.EqualTo("ENU"));
                Assert.That(speechMessagePacket.Name, Is.EqualTo("Tom"));
                Assert.That(speechMessagePacket.Text, Is.EqualTo("hello"));
            }
        );
    }
}
