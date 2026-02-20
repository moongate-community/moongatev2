using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Services.Characters;
using Moongate.Server.Services.Entities;
using Moongate.Server.Services.Persistence;
using Moongate.Server.Services.Timing;
using Moongate.Tests.Server.Support;
using Moongate.Tests.TestSupport;
using Moongate.UO.Data.Geometry;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Services.Names;
using Moongate.UO.Data.Services.Templates;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.Server;

public class CharacterServiceTests
{
    [Test]
    public async Task AddCharacterToAccountAsync_ShouldAddCharacterId_WhenAccountExists()
    {
        using var temp = new TempDirectory();
        var persistence = await CreatePersistenceServiceAsync(temp.Path);
        var service = CreateCharacterService(
            persistence,
            new()
        );
        var accountId = (Serial)0x00000101;
        var characterId = (Serial)0x00000201;

        await persistence.UnitOfWork.Accounts.UpsertAsync(
            new()
            {
                Id = accountId,
                Username = "acc-add-char",
                PasswordHash = "pw"
            }
        );

        var added = await service.AddCharacterToAccountAsync(accountId, characterId);
        var reloaded = await persistence.UnitOfWork.Accounts.GetByIdAsync(accountId);

        Assert.Multiple(
            () =>
            {
                Assert.That(added, Is.True);
                Assert.That(reloaded, Is.Not.Null);
                Assert.That(reloaded!.CharacterIds, Has.Count.EqualTo(1));
                Assert.That(reloaded.CharacterIds[0], Is.EqualTo(characterId));
            }
        );
    }

    [Test]
    public async Task AddCharacterToAccountAsync_ShouldReturnFalse_WhenCharacterAlreadyLinked()
    {
        using var temp = new TempDirectory();
        var persistence = await CreatePersistenceServiceAsync(temp.Path);
        var service = CreateCharacterService(
            persistence,
            new()
        );
        var accountId = (Serial)0x00000102;
        var characterId = (Serial)0x00000202;

        await persistence.UnitOfWork.Accounts.UpsertAsync(
            new()
            {
                Id = accountId,
                Username = "acc-add-duplicate",
                PasswordHash = "pw",
                CharacterIds = [characterId]
            }
        );

        var added = await service.AddCharacterToAccountAsync(accountId, characterId);
        var reloaded = await persistence.UnitOfWork.Accounts.GetByIdAsync(accountId);

        Assert.Multiple(
            () =>
            {
                Assert.That(added, Is.False);
                Assert.That(reloaded, Is.Not.Null);
                Assert.That(reloaded!.CharacterIds, Has.Count.EqualTo(1));
            }
        );
    }

    [Test]
    public async Task CreateCharacterAsync_ShouldCreateStarterBackpackAndHardcodedItems()
    {
        using var temp = new TempDirectory();
        var persistence = await CreatePersistenceServiceAsync(temp.Path);
        var service = CreateCharacterService(
            persistence,
            new()
        );

        var createdId = await service.CreateCharacterAsync(
                            new()
                            {
                                Name = "starter-mobile",
                                AccountId = (Serial)0x00000150,
                                IsPlayer = true
                            }
                        );

        var savedCharacter = await persistence.UnitOfWork.Mobiles.GetByIdAsync(createdId);
        var allItems = await persistence.UnitOfWork.Items.GetAllAsync();
        var equippedLayers = savedCharacter!.EquippedItemIds.Keys.ToHashSet();

        Assert.Multiple(
            () =>
            {
                Assert.That(savedCharacter.BackpackId.IsItem, Is.True);
                Assert.That(equippedLayers, Does.Contain(ItemLayerType.Backpack));
                Assert.That(equippedLayers, Does.Contain(ItemLayerType.Shirt));
                Assert.That(equippedLayers, Does.Contain(ItemLayerType.Pants));
                Assert.That(equippedLayers, Does.Contain(ItemLayerType.Shoes));
                Assert.That(allItems.Count, Is.GreaterThanOrEqualTo(5));
                Assert.That(
                    allItems.Any(item => item.ItemId == 0x0EED && item.ParentContainerId == savedCharacter.BackpackId),
                    Is.True
                );
            }
        );
    }

    [Test]
    public async Task GetCharacterAsync_ShouldReturnCharacter_WhenExists()
    {
        using var temp = new TempDirectory();
        var persistence = await CreatePersistenceServiceAsync(temp.Path);
        var service = CreateCharacterService(
            persistence,
            new()
        );
        var characterId = (Serial)0x00000210;

        await persistence.UnitOfWork.Mobiles.UpsertAsync(
            new()
            {
                Id = characterId,
                Name = "single-mobile",
                IsPlayer = true
            }
        );

        var character = await service.GetCharacterAsync(characterId);

        Assert.That(character, Is.Not.Null);
        Assert.That(character!.Id, Is.EqualTo(characterId));
    }

    [Test]
    public async Task GetCharacterAsync_ShouldHydrateEquippedItemReferences_WhenCharacterHasEquippedItems()
    {
        using var temp = new TempDirectory();
        var persistence = await CreatePersistenceServiceAsync(temp.Path);
        var service = CreateCharacterService(
            persistence,
            new()
        );
        var characterId = (Serial)0x00000211;
        var shirtId = (Serial)0x40000011;

        await persistence.UnitOfWork.Mobiles.UpsertAsync(
            new()
            {
                Id = characterId,
                Name = "equipped-mobile",
                IsPlayer = true,
                EquippedItemIds =
                {
                    [ItemLayerType.Shirt] = shirtId
                }
            }
        );

        await persistence.UnitOfWork.Items.UpsertAsync(
            new()
            {
                Id = shirtId,
                ItemId = 0x1517,
                Hue = 0x0444,
                EquippedMobileId = characterId,
                EquippedLayer = ItemLayerType.Shirt
            }
        );

        var character = await service.GetCharacterAsync(characterId);

        Assert.That(character, Is.Not.Null);
        Assert.That(character!.TryGetEquippedReference(ItemLayerType.Shirt, out var reference), Is.True);
        Assert.That(reference.ItemId, Is.EqualTo(0x1517));
        Assert.That(reference.Hue, Is.EqualTo(0x0444));
    }

    [Test]
    public async Task GetCharactersForAccountAsync_ShouldHydrateEquippedItemReferences_ForReturnedCharacters()
    {
        using var temp = new TempDirectory();
        var persistence = await CreatePersistenceServiceAsync(temp.Path);
        var service = CreateCharacterService(
            persistence,
            new()
        );
        var accountId = (Serial)0x00000141;
        var characterId = (Serial)0x00000241;
        var pantsId = (Serial)0x40000041;

        await persistence.UnitOfWork.Accounts.UpsertAsync(
            new()
            {
                Id = accountId,
                Username = "acc-hydrate-all",
                PasswordHash = "pw",
                CharacterIds = [characterId]
            }
        );

        await persistence.UnitOfWork.Mobiles.UpsertAsync(
            new()
            {
                Id = characterId,
                Name = "hydrated-list-mobile",
                IsPlayer = true,
                EquippedItemIds =
                {
                    [ItemLayerType.Pants] = pantsId
                }
            }
        );

        await persistence.UnitOfWork.Items.UpsertAsync(
            new()
            {
                Id = pantsId,
                ItemId = 0x152E,
                Hue = 0x0222,
                EquippedMobileId = characterId,
                EquippedLayer = ItemLayerType.Pants
            }
        );

        var characters = await service.GetCharactersForAccountAsync(accountId);

        Assert.That(characters, Has.Count.EqualTo(1));
        Assert.That(characters[0].TryGetEquippedReference(ItemLayerType.Pants, out var reference), Is.True);
        Assert.That(reference.ItemId, Is.EqualTo(0x152E));
        Assert.That(reference.Hue, Is.EqualTo(0x0222));
    }

    [Test]
    public async Task GetCharactersForAccountAsync_ShouldReturnOnlyExistingMobiles()
    {
        using var temp = new TempDirectory();
        var persistence = await CreatePersistenceServiceAsync(temp.Path);
        var service = CreateCharacterService(
            persistence,
            new()
        );
        var accountId = (Serial)0x00000104;
        var existingCharacterId = (Serial)0x00000204;
        var missingCharacterId = (Serial)0x00000205;

        await persistence.UnitOfWork.Accounts.UpsertAsync(
            new()
            {
                Id = accountId,
                Username = "acc-get-chars",
                PasswordHash = "pw",
                CharacterIds = [existingCharacterId, missingCharacterId]
            }
        );

        await persistence.UnitOfWork.Mobiles.UpsertAsync(
            new()
            {
                Id = existingCharacterId,
                Name = "existing-mobile",
                IsPlayer = true
            }
        );

        var characters = await service.GetCharactersForAccountAsync(accountId);

        Assert.That(characters, Has.Count.EqualTo(1));
        Assert.That(characters[0].Id, Is.EqualTo(existingCharacterId));
    }

    [Test]
    public async Task GetBackpackWithItemsAsync_ShouldReturnBackpackWithContainedItems()
    {
        using var temp = new TempDirectory();
        var persistence = await CreatePersistenceServiceAsync(temp.Path);
        var service = CreateCharacterService(
            persistence,
            new()
        );
        var characterId = (Serial)0x00000251;
        var backpackId = (Serial)0x40000051;
        var goldId = (Serial)0x40000052;

        var character = new UOMobileEntity
        {
            Id = characterId,
            Name = "pack-mobile",
            IsPlayer = true,
            BackpackId = backpackId
        };

        await persistence.UnitOfWork.Items.UpsertAsync(
            new()
            {
                Id = backpackId,
                ItemId = 0x0E75,
                EquippedMobileId = characterId,
                EquippedLayer = ItemLayerType.Backpack
            }
        );

        await persistence.UnitOfWork.Items.UpsertAsync(
            new()
            {
                Id = goldId,
                ItemId = 0x0EED,
                ParentContainerId = backpackId,
                ContainerPosition = new(11, 22)
            }
        );

        var backpack = await service.GetBackpackWithItemsAsync(character);

        Assert.That(backpack, Is.Not.Null);
        Assert.That(backpack!.Items.Count, Is.EqualTo(1));
        Assert.That(backpack.Items[0].Item.Id, Is.EqualTo(goldId));
        Assert.That(backpack.Items[0].Position, Is.EqualTo(new Point2D(11, 22)));
    }

    [Test]
    public async Task RemoveCharacterFromAccountAsync_ShouldRemoveLinkedCharacter()
    {
        using var temp = new TempDirectory();
        var persistence = await CreatePersistenceServiceAsync(temp.Path);
        var service = CreateCharacterService(
            persistence,
            new()
        );
        var accountId = (Serial)0x00000103;
        var characterId = (Serial)0x00000203;

        await persistence.UnitOfWork.Accounts.UpsertAsync(
            new()
            {
                Id = accountId,
                Username = "acc-remove-char",
                PasswordHash = "pw",
                CharacterIds = [characterId]
            }
        );

        var removed = await service.RemoveCharacterFromAccountAsync(accountId, characterId);
        var reloaded = await persistence.UnitOfWork.Accounts.GetByIdAsync(accountId);

        Assert.Multiple(
            () =>
            {
                Assert.That(removed, Is.True);
                Assert.That(reloaded, Is.Not.Null);
                Assert.That(reloaded!.CharacterIds, Is.Empty);
            }
        );
    }

    private static CharacterService CreateCharacterService(
        PersistenceService persistenceService,
        GameEventScriptBridgeTestGameEventBusService gameEventBusService
    )
    {
        var itemTemplateService = new ItemTemplateService();
        itemTemplateService.Upsert(
            new()
            {
                Id = "backpack",
                Name = "Backpack",
                Category = "containers",
                Description = "Backpack",
                ItemId = "0x0E75",
                Hue = HueSpec.FromValue(0),
                GoldValue = GoldValueSpec.FromValue(0),
                LootType = LootType.Regular,
                ScriptId = "none"
            }
        );
        itemTemplateService.Upsert(
            new()
            {
                Id = "gold",
                Name = "Gold",
                Category = "currency",
                Description = "Gold",
                ItemId = "0x0EED",
                Hue = HueSpec.FromValue(0),
                GoldValue = GoldValueSpec.FromValue(1),
                LootType = LootType.Regular,
                ScriptId = "none"
            }
        );

        var entityFactoryService = new EntityFactoryService(
            itemTemplateService,
            new MobileTemplateService(),
            new NameService(),
            persistenceService
        );

        return new(persistenceService, entityFactoryService, gameEventBusService);
    }

    private static async Task<PersistenceService> CreatePersistenceServiceAsync(string rootDirectory)
    {
        var directories = new DirectoriesConfig(rootDirectory, Enum.GetNames<DirectoryType>());
        var persistence = new PersistenceService(
            directories,
            new TimerWheelService(
                new()
                {
                    TickDuration = TimeSpan.FromMilliseconds(250),
                    WheelSize = 512
                }
            ),
            new()
        );
        await persistence.StartAsync();

        return persistence;
    }
}
