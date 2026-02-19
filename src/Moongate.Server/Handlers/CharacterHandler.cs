using Moongate.Network.Packets.Incoming.Login;
using Moongate.Network.Packets.Interfaces;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Characters;
using Moongate.Server.Interfaces.Services.Packets;
using Moongate.Server.Listeners.Base;
using Moongate.UO.Data.Ids;
using Serilog;

namespace Moongate.Server.Handlers;

public class CharacterHandler : BasePacketListener
{
    private readonly ILogger _logger = Log.ForContext<CharacterHandler>();

    private readonly ICharacterService _characterService;

    public CharacterHandler(
        IOutgoingPacketQueue outgoingPacketQueue,
        ICharacterService characterService
    ) : base(outgoingPacketQueue)
    {
        _characterService = characterService;
    }

    protected override async Task<bool> HandleCoreAsync(GameSession session, IGameNetworkPacket packet)
    {
        if (packet is CharacterCreationPacket characterCreationPacket)
        {
            return await HandleCharacterCreationPacketAsync(session, characterCreationPacket);
        }

        return true;
    }

    private async Task<bool> HandleCharacterCreationPacketAsync(
        GameSession session,
        CharacterCreationPacket characterCreationPacket
    )
    {
        var entity = characterCreationPacket.ToEntity(Serial.Zero, session.AccountId);

       var newCharacter = await _characterService.CreateCharacterAsync(entity);


        return true;
    }
}
