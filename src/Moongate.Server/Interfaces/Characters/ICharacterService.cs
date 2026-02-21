using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.Server.Interfaces.Characters;

/// <summary>
/// Defines character lifecycle operations and account-character associations.
/// </summary>
public interface ICharacterService
{
    /// <summary>
    /// Links an existing character to an account.
    /// </summary>
    /// <param name="accountId">Account serial identifier.</param>
    /// <param name="characterId">Character serial identifier.</param>
    /// <returns>
    /// <see langword="true" /> when the association is created; otherwise <see langword="false" />.
    /// </returns>
    Task<bool> AddCharacterToAccountAsync(Serial accountId, Serial characterId);

    /// <summary>
    /// Creates and persists a new character, returning the allocated character serial.
    /// </summary>
    /// <param name="character">Character entity to create.</param>
    /// <returns>The created character serial identifier.</returns>
    Task<Serial> CreateCharacterAsync(UOMobileEntity character);

    /// <summary>
    /// Loads a character entity by serial.
    /// </summary>
    /// <param name="characterId">Character serial identifier.</param>
    /// <returns>The character entity when found; otherwise <see langword="null" />.</returns>
    Task<UOMobileEntity?> GetCharacterAsync(Serial characterId);

    /// <summary>
    /// Loads all existing character entities linked to an account.
    /// </summary>
    /// <param name="accountId">Account serial identifier.</param>
    /// <returns>The list of linked character entities.</returns>
    Task<List<UOMobileEntity>> GetCharactersForAccountAsync(Serial accountId);

    /// <summary>
    /// Loads the character backpack with contained items for outbound packet serialization.
    /// </summary>
    /// <param name="character">Character entity.</param>
    /// <returns>The backpack entity including contained items when available.</returns>
    Task<UOItemEntity?> GetBackpackWithItemsAsync(UOMobileEntity character);

    /// <summary>
    /// Removes an existing account-character association.
    /// </summary>
    /// <param name="accountId">Account serial identifier.</param>
    /// <param name="characterId">Character serial identifier.</param>
    /// <returns>
    /// <see langword="true" /> when the association is removed; otherwise <see langword="false" />.
    /// </returns>
    Task<bool> RemoveCharacterFromAccountAsync(Serial accountId, Serial characterId);
}
