using Moongate.Network.Packets.Incoming.Movement;
using Moongate.Network.Packets.Interfaces;
using Moongate.Network.Packets.Outgoing.Movement;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Services.Packets;
using Moongate.Server.Listeners.Base;
using Moongate.UO.Data.Geometry;
using Moongate.UO.Data.Types;
using Serilog;

namespace Moongate.Server.Handlers;

public class MovementHandler : BasePacketListener
{
    private const long MovementThrottleResetMs = 1000;
    private const long MovementThrottleThresholdMs = 400;
    private const int WalkFootDelayMs = 400;
    private const int RunFootDelayMs = 200;
    private const int WalkMountDelayMs = 200;
    private const int RunMountDelayMs = 100;
    private const int TurnDelayMs = 100;

    private readonly ILogger _logger = Log.ForContext<MovementHandler>();

    public MovementHandler(IOutgoingPacketQueue outgoingPacketQueue)
        : base(outgoingPacketQueue)
    { }

    protected override Task<bool> HandleCoreAsync(GameSession session, IGameNetworkPacket packet)
    {
        if (packet is not MoveRequestPacket moveRequestPacket)
        {
            return Task.FromResult(true);

        }

        _logger.Debug(
            "Move request Session={SessionId} Dir={Direction} WalkDir={WalkDirection} Run={Run} Seq={Sequence} Key=0x{FastWalkKey:X8}",
            session.SessionId,
            moveRequestPacket.Direction,
            moveRequestPacket.WalkDirection,
            moveRequestPacket.IsRunning,
            moveRequestPacket.Sequence,
            moveRequestPacket.FastWalkKey
        );

        if (session.MoveSequence == 0 && moveRequestPacket.Sequence != 0)
        {
            // Match POL behavior: drop desynced packet without replying.
            return Task.FromResult(true);
        }

        if (session.Character is null)
        {
            return Task.FromResult(true);
        }

        if (IsThrottled(session))
        {
            Enqueue(
                session,
                new MoveDenyPacket(
                    moveRequestPacket.Sequence,
                    (short)session.Character.Location.X,
                    (short)session.Character.Location.Y,
                    session.Character.Direction,
                    (sbyte)session.Character.Location.Z
                )
            );

            return Task.FromResult(true);
        }

        var currentDirection = Point3D.GetBaseDirection(session.Character.Direction);
        var requestedDirection = moveRequestPacket.WalkDirection;
        var isFacingChangeOnly = currentDirection != requestedDirection;

        if (isFacingChangeOnly)
        {
            session.Character.Direction = requestedDirection;
        }
        else
        {
            session.Character.Location += moveRequestPacket.Direction;
            session.Character.Direction = moveRequestPacket.Direction;
        }

        var nextSequence = moveRequestPacket.Sequence + 1;

        if (nextSequence == 256)
        {
            nextSequence = 1;
        }

        session.MoveSequence = (byte)nextSequence;
        Enqueue(session, new MoveConfirmPacket(moveRequestPacket.Sequence, session.SelfNotoriety));
        session.MoveTime += isFacingChangeOnly ? TurnDelayMs : ComputeSpeedMs(session.IsMounted, moveRequestPacket.Direction);

        return Task.FromResult(true);
    }

    private static int ComputeSpeedMs(bool isMounted, DirectionType direction)
    {
        if (isMounted)
        {
            return (direction & DirectionType.Running) != 0 ? RunMountDelayMs : WalkMountDelayMs;
        }

        return (direction & DirectionType.Running) != 0 ? RunFootDelayMs : WalkFootDelayMs;
    }

    private static bool IsThrottled(GameSession session)
    {
        var now = Environment.TickCount64;
        var credit = session.MoveCredit;
        var nextMove = session.MoveTime;

        if (now - nextMove + MovementThrottleResetMs > 0)
        {
            session.MoveCredit = 0;
            session.MoveTime = now;

            return false;
        }

        var cost = nextMove - now;

        if (credit < cost)
        {
            return true;
        }

        session.MoveCredit = Math.Min(MovementThrottleThresholdMs, credit - cost);

        return false;
    }
}
