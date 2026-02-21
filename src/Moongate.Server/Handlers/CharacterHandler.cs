using Moongate.Network.Packets.Incoming.Login;
using Moongate.Network.Packets.Incoming.System;
using Moongate.Network.Packets.Interfaces;
using Moongate.Network.Packets.Outgoing.Entity;
using Moongate.Network.Packets.Outgoing.Login;
using Moongate.Network.Packets.Outgoing.World;
using Moongate.Server.Data.Events;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Characters;
using Moongate.Server.Interfaces.Services.Entities;
using Moongate.Server.Interfaces.Services.Events;
using Moongate.Server.Interfaces.Services.Packets;
using Moongate.Server.Interfaces.Services.Sessions;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.Server.Listeners.Base;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Types;
using Serilog;

namespace Moongate.Server.Handlers;

public class CharacterHandler : BasePacketListener, IGameEventListener<CharacterSelectedEvent>
{
    private readonly ILogger _logger = Log.ForContext<CharacterHandler>();
    private readonly ICharacterService _characterService;
    private readonly IEntityFactoryService _entityFactoryService;
    private readonly IGameNetworkSessionService _gameNetworkSessionService;

    public CharacterHandler(
        IOutgoingPacketQueue outgoingPacketQueue,
        ICharacterService characterService,
        IEntityFactoryService entityFactoryService,
        IGameEventBusService gameEventBusService,
        IGameNetworkSessionService gameNetworkSessionService
    ) : base(outgoingPacketQueue)
    {
        _characterService = characterService;
        _entityFactoryService = entityFactoryService;
        _gameNetworkSessionService = gameNetworkSessionService;
        gameEventBusService.RegisterListener(this);
    }

    public async Task HandleAsync(CharacterSelectedEvent gameEvent, CancellationToken cancellationToken = default)
    {
        if (_gameNetworkSessionService.TryGet(gameEvent.Sessionid, out var gameSession))
        {
            await HandleCharacterLoggedIn(gameSession, gameEvent.CharacterId);
        }
    }

    public async Task<bool> HandleCharacterLoggedIn(GameSession session, Serial characterId)
    {
        var character = await _characterService.GetCharacterAsync(characterId);

        if (character == null)
        {
            _logger.Error(
                "Failed to load character with ID {CharacterId} for session {SessionId}",
                characterId,
                session.SessionId
            );

            return false;
        }

        session.CharacterId = characterId;
        session.Character = character;
        session.MoveSequence = 0;
        session.SelfNotoriety = (byte)character.Notoriety;
        session.IsMounted = character.IsMounted;
        session.MoveCredit = 0;
        session.MoveTime = Environment.TickCount64;

        _logger.Information(
            "Character {CharacterName} (ID: {CharacterId}) logged in for session {SessionId}",
            character.Name,
            character.Id,
            session.SessionId
        );

        Enqueue(session, new ClientVersionPacket());
        Enqueue(session, new LoginConfirmPacket(character));
        Enqueue(session, new SupportFeaturesPacket());
        Enqueue(session, new DrawPlayerPacket(character));

        Enqueue(session, new MobileDrawPacket(character, character, true, true));
        EnqueueWornItems(session, character);
        await EnqueueBackpackAsync(session, character);

        Enqueue(session, new WarModePacket(character));
        Enqueue(session, GeneralInformationPacket.CreateSetCursorHueSetMap(character.Map));
        Enqueue(session, new OverallLightLevelPacket(LightLevelType.Day));
        Enqueue(session, new PersonalLightLevelPacket(LightLevelType.Day, character));
        Enqueue(session, new SeasonPacket(character.Map.Season));

        Enqueue(session, new LoginCompletePacket());

        Enqueue(session, new SetTimePacket());
        Enqueue(session, new SeasonPacket(character.Map.Season));

        Enqueue(session, GeneralInformationPacket.CreateSetCursorHueSetMap(character.Map));
        Enqueue(session, new PaperdollPacket(character));

        return true;
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
        var entity = _entityFactoryService.CreatePlayerMobile(characterCreationPacket, session.AccountId);

        entity.Title = "the grandmaster of moongate";
        var newCharacter = await _characterService.CreateCharacterAsync(entity);

        await _characterService.AddCharacterToAccountAsync(session.AccountId, newCharacter);

        await HandleCharacterLoggedIn(session, newCharacter);

        return true;
    }

    private void EnqueueWornItems(GameSession session, UOMobileEntity character)
    {
        foreach (var (layer, itemReference) in character.EquippedItemReferences)
        {
            if (layer == ItemLayerType.Backpack || layer == ItemLayerType.Bank)
            {
                continue;
            }

            Enqueue(session, new WornItemPacket(character, itemReference, layer));
        }
    }

    private async Task EnqueueBackpackAsync(GameSession session, UOMobileEntity character)
    {
        var backpack = await _characterService.GetBackpackWithItemsAsync(character);

        if (backpack is null)
        {
            return;
        }

        Enqueue(session, new DrawContainerAndAddItemCombinedPacket(backpack));
    }
}
