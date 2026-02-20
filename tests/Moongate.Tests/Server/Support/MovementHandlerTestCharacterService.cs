using Moongate.Server.Interfaces.Characters;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.Server.Support;

public sealed class MovementHandlerTestCharacterService : ICharacterService
{
    public Task<bool> AddCharacterToAccountAsync(Serial accountId, Serial characterId)
    {
        _ = accountId;
        _ = characterId;

        return Task.FromResult(true);
    }

    public Task<Serial> CreateCharacterAsync(UOMobileEntity character)
    {
        _ = character;

        return Task.FromResult((Serial)1u);
    }

    public Task<UOMobileEntity?> GetCharacterAsync(Serial characterId)
    {
        return Task.FromResult<UOMobileEntity?>(
            new()
            {
                Id = characterId,
                Notoriety = Notoriety.Innocent
            }
        );
    }

    public Task<List<UOMobileEntity>> GetCharactersForAccountAsync(Serial accountId)
    {
        _ = accountId;

        return Task.FromResult(new List<UOMobileEntity>());
    }

    public Task<UOItemEntity?> GetBackpackWithItemsAsync(UOMobileEntity character)
    {
        _ = character;

        return Task.FromResult<UOItemEntity?>(null);
    }

    public Task<bool> RemoveCharacterFromAccountAsync(Serial accountId, Serial characterId)
    {
        _ = accountId;
        _ = characterId;

        return Task.FromResult(true);
    }
}
