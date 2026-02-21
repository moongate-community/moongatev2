using Moongate.Network.Packets.Incoming.Player;
using Moongate.Network.Packets.Interfaces;
using Moongate.Network.Packets.Outgoing.Entity;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Characters;
using Moongate.Server.Interfaces.Services.Packets;
using Moongate.Server.Listeners.Base;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Types;

namespace Moongate.Server.Handlers;

public class GetPlayerStatusHandler : BasePacketListener
{
    private readonly ICharacterService _characterService;

    public GetPlayerStatusHandler(IOutgoingPacketQueue outgoingPacketQueue, ICharacterService characterService)
        : base(outgoingPacketQueue)
        => _characterService = characterService;

    protected override async Task<bool> HandleCoreAsync(GameSession session, IGameNetworkPacket packet)
    {
        if (packet is not GetPlayerStatusPacket getPlayerStatusPacket)
        {
            return true;
        }

        if (getPlayerStatusPacket.StatusType != GetPlayerStatusType.BasicStatus)
        {
            return true;
        }

        var target = await ResolveTargetMobileAsync(session, getPlayerStatusPacket.MobileSerial);

        if (target is null)
        {
            return true;
        }

        Enqueue(session, new PlayerStatusPacket(target));

        return true;
    }

    private async Task<UOMobileEntity?> ResolveTargetMobileAsync(GameSession session, uint requestedSerial)
    {
        if (session.Character is not null &&
            (requestedSerial == 0 || requestedSerial == session.Character.Id.Value))
        {
            return session.Character;
        }

        if (requestedSerial != 0)
        {
            return await _characterService.GetCharacterAsync((Serial)requestedSerial);
        }

        if (session.CharacterId != Serial.Zero)
        {
            return await _characterService.GetCharacterAsync(session.CharacterId);
        }

        return null;
    }
}
