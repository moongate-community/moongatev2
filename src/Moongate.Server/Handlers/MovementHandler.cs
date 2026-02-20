using Moongate.Network.Packets.Incoming.Movement;
using Moongate.Network.Packets.Interfaces;
using Moongate.Network.Packets.Outgoing.Movement;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Characters;
using Moongate.Server.Interfaces.Services.Packets;
using Moongate.Server.Listeners.Base;
using Moongate.UO.Data.Types;
using Serilog;

namespace Moongate.Server.Handlers;

public class MovementHandler : BasePacketListener
{
    private readonly ILogger _logger = Log.ForContext<MovementHandler>();

    private readonly ICharacterService _characterService;

    public MovementHandler(IOutgoingPacketQueue outgoingPacketQueue, ICharacterService characterService)
        : base(outgoingPacketQueue)
    {
        _characterService = characterService;
    }

    protected override async Task<bool> HandleCoreAsync(GameSession session, IGameNetworkPacket packet)
    {
        if (packet is not MoveRequestPacket moveRequestPacket)
        {
            return true;

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
            Enqueue(
                session,
                new MoveDenyPacket(
                    moveRequestPacket.Sequence,
                    0,
                    0,
                    moveRequestPacket.WalkDirection,
                    0
                )
            );
            session.MoveSequence = 0;

            return true;
        }

        var nextSequence = moveRequestPacket.Sequence + 1;

        if (nextSequence == 256)
        {
            nextSequence = 1;
        }

        var character = await _characterService.GetCharacterAsync(session.CharacterId);
        session.MoveSequence = (byte)nextSequence;
        Enqueue(session, new MoveConfirmPacket(moveRequestPacket.Sequence, (byte)character.Notoriety));

        return true;
    }
}
