using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Characters;
using Moongate.Server.Interfaces.Services.Events;
using Moongate.Server.Interfaces.Services.Persistence;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;
using Serilog;

namespace Moongate.Server.Services.Characters;

public class CharacterService : ICharacterService
{
    private readonly ILogger _logger = Log.ForContext<CharacterService>();
    private readonly IPersistenceService _persistenceService;

    private readonly IGameEventBusService _gameEventBusService;

    public CharacterService(IPersistenceService persistenceService, IGameEventBusService gameEventBusService)
    {
        _persistenceService = persistenceService;
        _gameEventBusService = gameEventBusService;
    }

    public async Task<Serial> CreateCharacterAsync(UOMobileEntity character)
    {
        character.Id = _persistenceService.UnitOfWork.AllocateNextMobileId();

        await _persistenceService.UnitOfWork.Mobiles.UpsertAsync(character);

        await _gameEventBusService.PublishAsync(
            new CharacterCreatedEvent(
                character.Name,
                character.AccountId,
                character.Id,
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            )
        );

        _logger.Debug("Created character {CharacterName}", character.Name);

        return character.Id;
    }

    public async Task<UOMobileEntity?> GetCharacterAsync(Serial characterId)
    {
        var character = await _persistenceService.UnitOfWork.Mobiles.GetByIdAsync(characterId);

        if (character is null)
        {
            _logger.Warning("Character {CharacterId} not found", characterId);
            return null;
        }

        _logger.Debug("Loaded character {CharacterId}", characterId);

        return character;
    }

    public async Task<bool> AddCharacterToAccountAsync(Serial accountId, Serial characterId)
    {
        var account = await _persistenceService.UnitOfWork.Accounts.GetByIdAsync(accountId);
        if (account == null)
        {
            _logger.Warning(
                "Cannot add character {CharacterId} to account {AccountId}: account not found",
                characterId,
                accountId
            );
            return false;
        }

        if (account.CharacterIds.Contains(characterId))
        {
            _logger.Warning(
                "Cannot add character {CharacterId} to account {AccountId}: character already linked",
                characterId,
                accountId
            );
            return false;
        }

        account.CharacterIds.Add(characterId);
        await _persistenceService.UnitOfWork.Accounts.UpsertAsync(account);

        _logger.Information("Added character {CharacterId} to account {AccountId}", characterId, accountId);
        return true;
    }

    public async Task<bool> RemoveCharacterFromAccountAsync(Serial accountId, Serial characterId)
    {
        var account = await _persistenceService.UnitOfWork.Accounts.GetByIdAsync(accountId);
        if (account == null)
        {
            _logger.Warning(
                "Cannot remove character {CharacterId} from account {AccountId}: account not found",
                characterId,
                accountId
            );
            return false;
        }

        var removed = account.CharacterIds.Remove(characterId);
        if (!removed)
        {
            _logger.Warning(
                "Cannot remove character {CharacterId} from account {AccountId}: character not linked",
                characterId,
                accountId
            );
            return false;
        }

        await _persistenceService.UnitOfWork.Accounts.UpsertAsync(account);
        _logger.Information("Removed character {CharacterId} from account {AccountId}", characterId, accountId);
        return true;
    }

    public async Task<List<UOMobileEntity>> GetCharactersForAccountAsync(Serial accountId)
    {
        var account = await _persistenceService.UnitOfWork.Accounts.GetByIdAsync(accountId);
        if (account == null)
        {
            _logger.Warning("Cannot get characters for account {AccountId}: account not found", accountId);
            return [];
        }

        var characters = new List<UOMobileEntity>(account.CharacterIds.Count);

        foreach (var characterId in account.CharacterIds)
        {
            var mobile = await _persistenceService.UnitOfWork.Mobiles.GetByIdAsync(characterId);
            if (mobile != null)
            {
                characters.Add(mobile);
            }
        }

        _logger.Information(
            "Retrieved {CharacterCount} characters for account {AccountId}",
            characters.Count,
            accountId
        );
        return characters;
    }
}
