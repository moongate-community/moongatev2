using System.Net.Sockets;
using Moongate.Network.Client;
using Moongate.Network.Packets.Incoming.Movement;
using Moongate.Network.Packets.Outgoing.Movement;
using Moongate.Server.Data.Session;
using Moongate.Server.Handlers;
using Moongate.Tests.Server.Support;
using Moongate.UO.Data.Geometry;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.Server;

public class MovementHandlerTests
{
    [Test]
    public async Task HandlePacketAsync_ShouldDropPacket_WhenFirstSequenceIsNotZero()
    {
        var queue = new BasePacketListenerTestOutgoingPacketQueue();
        var handler = new MovementHandler(queue);
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        var session = new GameSession(new(client))
        {
            MoveSequence = 0,
            CharacterId = (Serial)0x00000001,
            Character = new UOMobileEntity
            {
                Id = (Serial)0x00000001,
                Location = new(1200, 1300, 7),
                Direction = DirectionType.East
            }
        };
        var packet = new MoveRequestPacket
        {
            Direction = DirectionType.North,
            Sequence = 4,
            FastWalkKey = 0x11223344
        };

        var handled = await handler.HandlePacketAsync(session, packet);
        var dequeued = queue.TryDequeue(out _);

        Assert.Multiple(
            () =>
            {
                Assert.That(handled, Is.True);
                Assert.That(dequeued, Is.False);
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
        var session = new GameSession(new(client))
        {
            CharacterId = (Serial)0x00000001,
            Character = new UOMobileEntity
            {
                Id = (Serial)0x00000001,
                Location = new(1200, 1300, 7),
                Direction = DirectionType.East
            }
        };
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
                Assert.That(session.Character, Is.Not.Null);
                Assert.That(session.Character!.Direction, Is.EqualTo(DirectionType.East | DirectionType.Running));
                Assert.That(session.Character.Location, Is.EqualTo(new Point3D(1201, 1300, 7)));
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
            MoveCredit = 0,
            Character = new UOMobileEntity
            {
                Id = (Serial)0x00000001,
                Location = new(1200, 1300, 7),
                Direction = DirectionType.East
            }
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
                var deny = (MoveDenyPacket)outbound.Packet;
                Assert.That(deny.X, Is.EqualTo(1200));
                Assert.That(deny.Y, Is.EqualTo(1300));
                Assert.That(deny.Z, Is.EqualTo(7));
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
            IsMounted = false,
            Character = new UOMobileEntity
            {
                Id = (Serial)0x00000001,
                Location = new(1200, 1300, 7),
                Direction = DirectionType.East
            }
        };
        var runSession = new GameSession(new(client))
        {
            CharacterId = (Serial)0x00000002,
            MoveSequence = 0,
            MoveTime = 0,
            IsMounted = false,
            Character = new UOMobileEntity
            {
                Id = (Serial)0x00000002,
                Location = new(1200, 1300, 7),
                Direction = DirectionType.East
            }
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

    [Test]
    public async Task HandlePacketAsync_ShouldOnlyTurnWithoutMoving_WhenFacingChanges()
    {
        var queue = new BasePacketListenerTestOutgoingPacketQueue();
        var handler = new MovementHandler(queue);
        using var client = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        var session = new GameSession(new(client))
        {
            CharacterId = (Serial)0x00000001,
            Character = new UOMobileEntity
            {
                Id = (Serial)0x00000001,
                Location = new(500, 500, 0),
                Direction = DirectionType.North
            }
        };

        _ = await handler.HandlePacketAsync(
            session,
            new MoveRequestPacket
            {
                Direction = DirectionType.East,
                Sequence = 0
            }
        );
        _ = queue.TryDequeue(out _);

        Assert.Multiple(
            () =>
            {
                Assert.That(session.Character, Is.Not.Null);
                Assert.That(session.Character!.Direction, Is.EqualTo(DirectionType.East));
                Assert.That(session.Character.Location, Is.EqualTo(new Point3D(500, 500, 0)));
            }
        );
    }
}
