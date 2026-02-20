using Moongate.Network.Packets.Incoming.Login;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.Server.Interfaces.Services.Entities;

/// <summary>
/// Creates runtime entities from packets and template definitions.
/// </summary>
public interface IEntityFactoryService
{
    /// <summary>
    /// Creates an item entity from an item template id.
    /// </summary>
    /// <param name="itemTemplateId">Item template identifier.</param>
    /// <returns>Initialized item entity with allocated serial.</returns>
    UOItemEntity CreateItemFromTemplate(string itemTemplateId);

    /// <summary>
    /// Creates a mobile entity from a mobile template id.
    /// </summary>
    /// <param name="mobileTemplateId">Mobile template identifier.</param>
    /// <param name="accountId">Optional owner account identifier for player mobiles.</param>
    /// <returns>Initialized mobile entity with allocated serial.</returns>
    UOMobileEntity CreateMobileFromTemplate(string mobileTemplateId, Serial? accountId = null);

    /// <summary>
    /// Creates a player mobile from character creation packet data.
    /// </summary>
    /// <param name="packet">Character creation packet.</param>
    /// <param name="accountId">Owner account serial identifier.</param>
    /// <returns>Initialized mobile entity with allocated serial.</returns>
    UOMobileEntity CreatePlayerMobile(CharacterCreationPacket packet, Serial accountId);

    /// <summary>
    /// Creates a backpack item for newly created characters.
    /// </summary>
    /// <returns>Initialized backpack item entity with allocated serial.</returns>
    UOItemEntity GetNewBackpack();
}
