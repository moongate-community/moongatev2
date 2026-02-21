using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Characters;
using Moongate.Server.Interfaces.Services.Entities;
using Moongate.Server.Interfaces.Services.Events;
using Moongate.Server.Interfaces.Services.Persistence;
using Moongate.UO.Data.Geometry;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Types;
using Serilog;

namespace Moongate.Server.Services.Characters;

public class CharacterService : ICharacterService
{
    private const int BackpackItemId = 0x0E75;
    private const int GoldItemId = 0x0EED;
    private const int ShirtItemId = 0x1517;
    private const int PantsItemId = 0x152E;
    private const int ShoesItemId = 0x170F;

    private readonly ILogger _logger = Log.ForContext<CharacterService>();
    private readonly IPersistenceService _persistenceService;
    private readonly IEntityFactoryService _entityFactoryService;
    private readonly IGameEventBusService _gameEventBusService;

    public CharacterService(
        IPersistenceService persistenceService,
        IEntityFactoryService entityFactoryService,
        IGameEventBusService gameEventBusService
    )
    {
        _persistenceService = persistenceService;
        _entityFactoryService = entityFactoryService;
        _gameEventBusService = gameEventBusService;
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

    public async Task<Serial> CreateCharacterAsync(UOMobileEntity character)
    {
        character.Id = _persistenceService.UnitOfWork.AllocateNextMobileId();
        await EnsureStarterInventoryAsync(character);

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

        await HydrateCharacterEquipmentRuntimeAsync(character);

        _logger.Debug("Loaded character {CharacterId}", characterId);

        return character;
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
                await HydrateCharacterEquipmentRuntimeAsync(mobile);
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

    public async Task<UOItemEntity?> GetBackpackWithItemsAsync(UOMobileEntity character)
    {
        ArgumentNullException.ThrowIfNull(character);

        var backpackId = character.BackpackId;

        if (backpackId == Serial.Zero &&
            character.EquippedItemIds.TryGetValue(ItemLayerType.Backpack, out var equippedBackpackId))
        {
            backpackId = equippedBackpackId;
        }

        if (backpackId == Serial.Zero)
        {
            return null;
        }

        var backpack = await _persistenceService.UnitOfWork.Items.GetByIdAsync(backpackId);

        if (backpack is null)
        {
            return null;
        }

        var hydratedBackpack = CloneItem(backpack);
        var containedItems = await _persistenceService.UnitOfWork.Items.QueryAsync(
                                 item => item.ParentContainerId == backpackId,
                                 static item => item
                             );

        foreach (var item in containedItems)
        {
            hydratedBackpack.AddItem(CloneItem(item), item.ContainerPosition);
        }

        return hydratedBackpack;
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

    private async Task EnsureStarterContainerItemAsync(
        UOMobileEntity character,
        Serial containerId,
        int itemId,
        Point2D containerPosition
    )
    {
        var existing = await _persistenceService.UnitOfWork.Items.QueryAsync(
                           item => item.ParentContainerId == containerId && item.ItemId == itemId,
                           static item => item
                       );

        if (existing.Count > 0)
        {
            return;
        }

        var item = new UOItemEntity
        {
            Id = _persistenceService.UnitOfWork.AllocateNextItemId(),
            ItemId = itemId,
            Hue = 0,
            Location = Point3D.Zero,
            ParentContainerId = containerId,
            ContainerPosition = containerPosition,
            EquippedMobileId = Serial.Zero,
            EquippedLayer = null
        };

        await _persistenceService.UnitOfWork.Items.UpsertAsync(item);
        _logger.Debug("Created starter container item {ItemId:X4} for character {CharacterId}", itemId, character.Id);
    }

    private async Task EnsureStarterEquippedItemAsync(UOMobileEntity character, ItemLayerType layer, int itemId)
    {
        if (character.HasEquippedItem(layer))
        {
            return;
        }

        var item = new UOItemEntity
        {
            Id = _persistenceService.UnitOfWork.AllocateNextItemId(),
            ItemId = itemId,
            Hue = 0,
            Location = Point3D.Zero,
            ParentContainerId = Serial.Zero,
            ContainerPosition = Point2D.Zero,
            EquippedMobileId = character.Id,
            EquippedLayer = layer
        };

        character.AddEquippedItem(layer, item);
        await _persistenceService.UnitOfWork.Items.UpsertAsync(item);
        _logger.Debug(
            "Created starter equipped item {ItemId:X4} on layer {Layer} for {CharacterId}",
            itemId,
            layer,
            character.Id
        );
    }

    private async Task EnsureStarterInventoryAsync(UOMobileEntity character)
    {
        UOItemEntity backpack;

        if (!character.HasEquippedItem(ItemLayerType.Backpack))
        {
            backpack = _entityFactoryService.GetNewBackpack();
            character.AddEquippedItem(ItemLayerType.Backpack, backpack);
            character.BackpackId = backpack.Id;
        }
        else
        {
            character.BackpackId = character.EquippedItemIds[ItemLayerType.Backpack];
            backpack = await _persistenceService.UnitOfWork.Items.GetByIdAsync(character.BackpackId) ??
                       new UOItemEntity
                       {
                           Id = character.BackpackId,
                           ItemId = BackpackItemId,
                           Hue = 0,
                           Location = Point3D.Zero,
                           ParentContainerId = Serial.Zero,
                           ContainerPosition = Point2D.Zero,
                           EquippedMobileId = character.Id,
                           EquippedLayer = ItemLayerType.Backpack
                       };
        }

        await _persistenceService.UnitOfWork.Items.UpsertAsync(backpack);

        await EnsureStarterContainerItemAsync(character, backpack.Id, GoldItemId, new(1, 1));
        await EnsureStarterEquippedItemAsync(character, ItemLayerType.Shirt, ShirtItemId);
        await EnsureStarterEquippedItemAsync(character, ItemLayerType.Pants, PantsItemId);
        await EnsureStarterEquippedItemAsync(character, ItemLayerType.Shoes, ShoesItemId);
    }

    private async Task HydrateCharacterEquipmentRuntimeAsync(UOMobileEntity character)
    {
        ArgumentNullException.ThrowIfNull(character);

        if (character.EquippedItemIds.Count == 0)
        {
            character.HydrateEquipmentRuntime([]);

            return;
        }

        var equippedItems = await _persistenceService.UnitOfWork.Items.QueryAsync(
                                item => item.EquippedMobileId == character.Id && item.EquippedLayer is not null,
                                static item => item
                            );

        var hydratedItems = equippedItems.ToDictionary(static item => item.Id, static item => item);
        var inferredItems = new List<UOItemEntity>(character.EquippedItemIds.Count);

        foreach (var (layer, itemId) in character.EquippedItemIds)
        {
            if (hydratedItems.ContainsKey(itemId))
            {
                continue;
            }

            var item = await _persistenceService.UnitOfWork.Items.GetByIdAsync(itemId);

            if (item is null)
            {
                continue;
            }

            item.EquippedMobileId = character.Id;
            item.EquippedLayer = layer;
            inferredItems.Add(item);
        }

        if (inferredItems.Count > 0)
        {
            character.HydrateEquipmentRuntime([.. equippedItems, .. inferredItems]);

            return;
        }

        character.HydrateEquipmentRuntime(equippedItems);
    }

    private static UOItemEntity CloneItem(UOItemEntity item)
    {
        return new()
        {
            Id = item.Id,
            Location = item.Location,
            ItemId = item.ItemId,
            Hue = item.Hue,
            GumpId = item.GumpId,
            ParentContainerId = item.ParentContainerId,
            ContainerPosition = item.ContainerPosition,
            EquippedMobileId = item.EquippedMobileId,
            EquippedLayer = item.EquippedLayer
        };
    }
}
