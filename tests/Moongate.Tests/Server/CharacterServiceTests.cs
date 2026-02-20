using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Data.Config;
using Moongate.Server.Services.Characters;
using Moongate.Server.Services.Persistence;
using Moongate.Server.Services.Timing;
using Moongate.Tests.TestSupport;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.Tests.Server;

public class CharacterServiceTests
{
    [Test]
    public async Task GetCharacterAsync_ShouldReturnCharacter_WhenExists()
    {
        using var temp = new TempDirectory();
        var persistence = await CreatePersistenceServiceAsync(temp.Path);
        var service = new CharacterService(
            persistence,
            new Support.GameEventScriptBridgeTestGameEventBusService()
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
    public async Task AddCharacterToAccountAsync_ShouldAddCharacterId_WhenAccountExists()
    {
        using var temp = new TempDirectory();
        var persistence = await CreatePersistenceServiceAsync(temp.Path);
        var service = new CharacterService(
            persistence,
            new Support.GameEventScriptBridgeTestGameEventBusService()
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
        var service = new CharacterService(
            persistence,
            new Support.GameEventScriptBridgeTestGameEventBusService()
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
    public async Task RemoveCharacterFromAccountAsync_ShouldRemoveLinkedCharacter()
    {
        using var temp = new TempDirectory();
        var persistence = await CreatePersistenceServiceAsync(temp.Path);
        var service = new CharacterService(
            persistence,
            new Support.GameEventScriptBridgeTestGameEventBusService()
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

    [Test]
    public async Task GetCharactersForAccountAsync_ShouldReturnOnlyExistingMobiles()
    {
        using var temp = new TempDirectory();
        var persistence = await CreatePersistenceServiceAsync(temp.Path);
        var service = new CharacterService(
            persistence,
            new Support.GameEventScriptBridgeTestGameEventBusService()
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

    private static async Task<PersistenceService> CreatePersistenceServiceAsync(string rootDirectory)
    {
        var directories = new DirectoriesConfig(rootDirectory, Enum.GetNames<DirectoryType>());
        var persistence = new PersistenceService(
            directories,
            new TimerWheelService(
                new TimerServiceConfig
                {
                    TickDuration = TimeSpan.FromMilliseconds(250),
                    WheelSize = 512
                }
            ),
            new MoongateConfig()
        );
        await persistence.StartAsync();
        return persistence;
    }
}
