using System.Net.Sockets;
using Moongate.Network.Client;
using Moongate.Network.Packets.Incoming.Movement;
using Moongate.Network.Packets.Outgoing.Movement;
using Moongate.Server.Data.Session;
using Moongate.Server.Handlers;
using Moongate.Tests.Server.Support;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.Server;

public class MovementHandlerTests
{
    [Test]
    public async Task HandlePacketAsync_ShouldSendMoveDeny_WhenFirstSequenceIsNotZero()
    {
        var queue = new BasePacketListenerTestOutgoingPacketQueue();
        var handler = new MovementHandler(queue);
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        var session = new GameSession(new(client)) { MoveSequence = 0, CharacterId = (Serial)0x00000001 };
        var packet = new MoveRequestPacket
        {
            Direction = DirectionType.North,
            Sequence = 4,
            FastWalkKey = 0x11223344
        };

        var handled = await handler.HandlePacketAsync(session, packet);
        var dequeued = queue.TryDequeue(out var outbound);

        Assert.Multiple(
            () =>
            {
                Assert.That(handled, Is.True);
                Assert.That(dequeued, Is.True);
                Assert.That(outbound.Packet, Is.TypeOf<MoveDenyPacket>());
                Assert.That(session.MoveSequence, Is.EqualTo(0));
            }
        );
    }

    [Test]
    public async Task HandlePacketAsync_ShouldAckAndAdvanceSequence_WhenSequenceIsValid()
    {
        var queue = new BasePacketListenerTestOutgoingPacketQueue();
        var handler = new MovementHandler(queue);
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        var session = new GameSession(new(client)) { CharacterId = (Serial)0x00000001 };
        var packet = new MoveRequestPacket
        {
            Direction = DirectionType.East | DirectionType.Running,
            Sequence = 0,
            FastWalkKey = 0x55667788
        };

        var handled = await handler.HandlePacketAsync(session, packet);
        var dequeued = queue.TryDequeue(out var outbound);

        Assert.Multiple(
            () =>
            {
                Assert.That(handled, Is.True);
                Assert.That(dequeued, Is.True);
                Assert.That(outbound.Packet, Is.TypeOf<MoveConfirmPacket>());
                Assert.That(session.MoveSequence, Is.EqualTo(1));
            }
        );
    }

    [Test]
    public async Task HandlePacketAsync_ShouldThrottle_WhenMoveTimeIsFarAhead()
    {
        var queue = new BasePacketListenerTestOutgoingPacketQueue();
        var handler = new MovementHandler(queue);
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        var session = new GameSession(new(client))
        {
            CharacterId = (Serial)0x00000001,
            MoveSequence = 1,
            MoveTime = Environment.TickCount64 + 2000,
            MoveCredit = 0
        };

        var packet = new MoveRequestPacket
        {
            Direction = DirectionType.East,
            Sequence = 1,
            FastWalkKey = 0x01020305
        };

        _ = await handler.HandlePacketAsync(session, packet);
        var dequeued = queue.TryDequeue(out var outbound);

        Assert.Multiple(
            () =>
            {
                Assert.That(dequeued, Is.True);
                Assert.That(outbound.Packet, Is.TypeOf<MoveDenyPacket>());
                Assert.That(session.MoveSequence, Is.EqualTo(1));
            }
        );
    }

    [Test]
    public async Task HandlePacketAsync_ShouldApplyFasterDelayForRunning()
    {
        var queue = new BasePacketListenerTestOutgoingPacketQueue();
        var handler = new MovementHandler(queue);
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));

        var walkSession = new GameSession(new(client))
        {
            CharacterId = (Serial)0x00000001,
            MoveSequence = 0,
            MoveTime = 0,
            IsMounted = false
        };
        var runSession = new GameSession(new(client))
        {
            CharacterId = (Serial)0x00000002,
            MoveSequence = 0,
            MoveTime = 0,
            IsMounted = false
        };

        _ = await handler.HandlePacketAsync(
            walkSession,
            new MoveRequestPacket
            {
                Direction = DirectionType.East,
                Sequence = 0
            }
        );
        _ = queue.TryDequeue(out _);

        _ = await handler.HandlePacketAsync(
            runSession,
            new MoveRequestPacket
            {
                Direction = DirectionType.East | DirectionType.Running,
                Sequence = 0
            }
        );
        _ = queue.TryDequeue(out _);

        Assert.That(runSession.MoveTime, Is.LessThan(walkSession.MoveTime - 100));
    }
}
