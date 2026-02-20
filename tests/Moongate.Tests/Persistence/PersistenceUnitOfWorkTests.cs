using Moongate.Persistence.Data.Persistence;
using Moongate.Persistence.Services.Persistence;
using Moongate.Tests.TestSupport;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.Persistence;

public class PersistenceUnitOfWorkTests
{
    private static readonly string[] ExpectedAccountUsernames = ["admin", "alpha"];

    [Test]
    public async Task Accounts_ShouldUseSerialAsEntityId()
    {
        using var tempDirectory = new TempDirectory();
        var unitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await unitOfWork.InitializeAsync();

        var account = new UOAccountEntity
        {
            Id = (Serial)0x00000001,
            Username = "tester",
            PasswordHash = "hash"
        };

        await unitOfWork.Accounts.UpsertAsync(account);

        var loadedById = await unitOfWork.Accounts.GetByIdAsync((Serial)0x00000001);
        var loadedByName = await unitOfWork.Accounts.GetByUsernameAsync("tester");

        Assert.Multiple(
            () =>
            {
                Assert.That(loadedById, Is.Not.Null);
                Assert.That(loadedByName, Is.Not.Null);
                Assert.That(loadedById!.Id, Is.EqualTo((Serial)0x00000001));
                Assert.That(loadedByName!.Id, Is.EqualTo((Serial)0x00000001));
            }
        );
    }

    [Test]
    public async Task AddAsync_ShouldRejectDuplicateUsername()
    {
        using var tempDirectory = new TempDirectory();
        var unitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await unitOfWork.InitializeAsync();

        var first = await unitOfWork.Accounts.AddAsync(
                        new()
                        {
                            Id = (Serial)0x00000005,
                            Username = "dupe-user",
                            PasswordHash = "pw"
                        }
                    );

        var second = await unitOfWork.Accounts.AddAsync(
                         new()
                         {
                             Id = (Serial)0x00000006,
                             Username = "dupe-user",
                             PasswordHash = "pw"
                         }
                     );

        Assert.Multiple(
            () =>
            {
                Assert.That(first, Is.True);
                Assert.That(second, Is.False);
            }
        );
    }

    [Test]
    public async Task AllocateNextIds_AfterReload_ShouldContinueFromMaxPersistedIds()
    {
        using var tempDirectory = new TempDirectory();
        var firstUnitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await firstUnitOfWork.InitializeAsync();

        await firstUnitOfWork.Accounts.UpsertAsync(
            new()
            {
                Id = (Serial)0x00000010,
                Username = "allocator-account",
                PasswordHash = "pw"
            }
        );

        await firstUnitOfWork.Mobiles.UpsertAsync(
            new()
            {
                Id = (Serial)0x00000020,
                IsPlayer = true,
                IsAlive = true
            }
        );

        await firstUnitOfWork.Items.UpsertAsync(
            new()
            {
                Id = (Serial)(Serial.ItemOffset + 10),
                ItemId = 0x0EED
            }
        );

        await firstUnitOfWork.SaveSnapshotAsync();

        var secondUnitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await secondUnitOfWork.InitializeAsync();

        var nextAccountId = secondUnitOfWork.AllocateNextAccountId();
        var nextMobileId = secondUnitOfWork.AllocateNextMobileId();
        var nextItemId = secondUnitOfWork.AllocateNextItemId();

        Assert.Multiple(
            () =>
            {
                Assert.That(nextAccountId, Is.EqualTo((Serial)0x00000011));
                Assert.That(nextMobileId, Is.EqualTo((Serial)0x00000021));
                Assert.That(nextItemId, Is.EqualTo((Serial)(Serial.ItemOffset + 11)));
            }
        );
    }

    [Test]
    public async Task AllocateNextIds_ShouldReturnProgressiveValuesPerEntityType()
    {
        using var tempDirectory = new TempDirectory();
        var unitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await unitOfWork.InitializeAsync();

        var account1 = unitOfWork.AllocateNextAccountId();
        var account2 = unitOfWork.AllocateNextAccountId();
        var mobile1 = unitOfWork.AllocateNextMobileId();
        var mobile2 = unitOfWork.AllocateNextMobileId();
        var item1 = unitOfWork.AllocateNextItemId();
        var item2 = unitOfWork.AllocateNextItemId();

        Assert.Multiple(
            () =>
            {
                Assert.That(account1, Is.EqualTo((Serial)0x00000001));
                Assert.That(account2, Is.EqualTo((Serial)0x00000002));
                Assert.That(mobile1, Is.EqualTo((Serial)0x00000001));
                Assert.That(mobile2, Is.EqualTo((Serial)0x00000002));
                Assert.That(item1, Is.EqualTo((Serial)Serial.ItemOffset));
                Assert.That(item2, Is.EqualTo((Serial)(Serial.ItemOffset + 1)));
            }
        );
    }

    [Test]
    public async Task ConcurrentAccountUpserts_ShouldRemainConsistentAfterReload()
    {
        using var tempDirectory = new TempDirectory();
        var unitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await unitOfWork.InitializeAsync();

        const int taskCount = 16;
        const int writesPerTask = 50;

        var tasks = Enumerable.Range(0, taskCount)
                              .Select(
                                  taskIndex => Task.Run(
                                      async () =>
                                      {
                                          for (var i = 0; i < writesPerTask; i++)
                                          {
                                              var globalIndex = taskIndex * writesPerTask + i;

                                              await unitOfWork.Accounts.UpsertAsync(
                                                  new()
                                                  {
                                                      Id = (Serial)(uint)(0x00010000 + globalIndex),
                                                      Username = $"concurrent-{globalIndex}",
                                                      PasswordHash = "pw"
                                                  }
                                              );
                                          }
                                      }
                                  )
                              )
                              .ToArray();

        await Task.WhenAll(tasks);
        await unitOfWork.SaveSnapshotAsync();

        var reloadedUnitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await reloadedUnitOfWork.InitializeAsync();

        var accounts = await reloadedUnitOfWork.Accounts.GetAllAsync();

        Assert.That(accounts.Count, Is.EqualTo(taskCount * writesPerTask));

        for (var i = 0; i < taskCount * writesPerTask; i++)
        {
            var username = $"concurrent-{i}";
            var loaded = await reloadedUnitOfWork.Accounts.GetByUsernameAsync(username);

            Assert.That(loaded, Is.Not.Null, $"Missing account '{username}' after concurrent writes.");
        }
    }

    [Test]
    public async Task ConcurrentWritersAcrossMultipleUnitOfWorkInstances_ShouldRemainConsistentAfterReload()
    {
        using var tempDirectory = new TempDirectory();
        var firstUnitOfWork = CreateUnitOfWork(tempDirectory.Path);
        var secondUnitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await firstUnitOfWork.InitializeAsync();
        await secondUnitOfWork.InitializeAsync();

        const int writesPerWriter = 150;

        var firstWriterTask = WriteAccountsWithRetryAsync(firstUnitOfWork, 1_000, writesPerWriter);
        var secondWriterTask = WriteAccountsWithRetryAsync(secondUnitOfWork, 2_000, writesPerWriter);

        await Task.WhenAll(firstWriterTask, secondWriterTask);

        var reloadedUnitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await reloadedUnitOfWork.InitializeAsync();

        var accounts = await reloadedUnitOfWork.Accounts.GetAllAsync();
        Assert.That(accounts.Count, Is.EqualTo(writesPerWriter * 2));

        for (var i = 0; i < writesPerWriter; i++)
        {
            var firstUsername = $"multi-uow-{1_000 + i}";
            var secondUsername = $"multi-uow-{2_000 + i}";

            Assert.That(
                await reloadedUnitOfWork.Accounts.GetByUsernameAsync(firstUsername),
                Is.Not.Null,
                $"Missing account '{firstUsername}' after multi-instance writes."
            );
            Assert.That(
                await reloadedUnitOfWork.Accounts.GetByUsernameAsync(secondUsername),
                Is.Not.Null,
                $"Missing account '{secondUsername}' after multi-instance writes."
            );
        }
    }

    [Test]
    public async Task CountAsync_OnRepositories_ShouldReturnExpectedValues()
    {
        using var tempDirectory = new TempDirectory();
        var unitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await unitOfWork.InitializeAsync();

        await unitOfWork.Accounts.UpsertAsync(new() { Id = (Serial)0x1201, Username = "count-a", PasswordHash = "pw" });
        await unitOfWork.Accounts.UpsertAsync(new() { Id = (Serial)0x1202, Username = "count-b", PasswordHash = "pw" });
        await unitOfWork.Mobiles.UpsertAsync(new() { Id = (Serial)0x2201, IsPlayer = true, IsAlive = true });
        await unitOfWork.Items.UpsertAsync(new() { Id = (Serial)0x3201, ItemId = 0x0EED });
        await unitOfWork.Items.UpsertAsync(new() { Id = (Serial)0x3202, ItemId = 0x0F3F });
        await unitOfWork.Items.UpsertAsync(new() { Id = (Serial)0x3203, ItemId = 0x0EED });

        var accountCount = await unitOfWork.Accounts.CountAsync();
        var mobileCount = await unitOfWork.Mobiles.CountAsync();
        var itemCount = await unitOfWork.Items.CountAsync();

        Assert.Multiple(
            () =>
            {
                Assert.That(accountCount, Is.EqualTo(2));
                Assert.That(mobileCount, Is.EqualTo(1));
                Assert.That(itemCount, Is.EqualTo(3));
            }
        );
    }

    [Test]
    public async Task ExistsAsync_OnAccounts_ShouldReturnExpectedValue()
    {
        using var tempDirectory = new TempDirectory();
        var unitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await unitOfWork.InitializeAsync();

        await unitOfWork.Accounts.UpsertAsync(new() { Id = (Serial)0x1101, Username = "exists-user", PasswordHash = "pw" });

        var exists = await unitOfWork.Accounts.ExistsAsync(account => account.Username == "exists-user");
        var notExists = await unitOfWork.Accounts.ExistsAsync(account => account.Username == "missing-user");

        Assert.Multiple(
            () =>
            {
                Assert.That(exists, Is.True);
                Assert.That(notExists, Is.False);
            }
        );
    }

    [Test]
    public async Task InitializeAsync_ShouldPreserveAccountEmailAcrossSnapshotAndJournalReplay()
    {
        using var tempDirectory = new TempDirectory();
        var unitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await unitOfWork.InitializeAsync();

        await unitOfWork.Accounts.UpsertAsync(
            new()
            {
                Id = (Serial)0x00000050,
                Username = "snapshot-email",
                PasswordHash = "pw",
                Email = "snapshot@moongate.local"
            }
        );

        await unitOfWork.SaveSnapshotAsync();

        await unitOfWork.Accounts.UpsertAsync(
            new()
            {
                Id = (Serial)0x00000051,
                Username = "journal-email",
                PasswordHash = "pw",
                Email = "journal@moongate.local"
            }
        );

        var secondUnitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await secondUnitOfWork.InitializeAsync();

        var snapshotAccount = await secondUnitOfWork.Accounts.GetByUsernameAsync("snapshot-email");
        var journalAccount = await secondUnitOfWork.Accounts.GetByUsernameAsync("journal-email");

        Assert.Multiple(
            () =>
            {
                Assert.That(snapshotAccount, Is.Not.Null);
                Assert.That(journalAccount, Is.Not.Null);
                Assert.That(snapshotAccount!.Email, Is.EqualTo("snapshot@moongate.local"));
                Assert.That(journalAccount!.Email, Is.EqualTo("journal@moongate.local"));
            }
        );
    }

    [Test]
    public async Task InitializeAsync_ShouldPreserveAccountLockStateAcrossSnapshotAndJournalReplay()
    {
        using var tempDirectory = new TempDirectory();
        var unitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await unitOfWork.InitializeAsync();

        await unitOfWork.Accounts.UpsertAsync(
            new()
            {
                Id = (Serial)0x00000040,
                Username = "snapshot-locked",
                PasswordHash = "pw",
                IsLocked = true
            }
        );

        await unitOfWork.SaveSnapshotAsync();

        await unitOfWork.Accounts.UpsertAsync(
            new()
            {
                Id = (Serial)0x00000041,
                Username = "journal-unlocked",
                PasswordHash = "pw",
                IsLocked = false
            }
        );

        var secondUnitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await secondUnitOfWork.InitializeAsync();

        var snapshotAccount = await secondUnitOfWork.Accounts.GetByUsernameAsync("snapshot-locked");
        var journalAccount = await secondUnitOfWork.Accounts.GetByUsernameAsync("journal-unlocked");

        Assert.Multiple(
            () =>
            {
                Assert.That(snapshotAccount, Is.Not.Null);
                Assert.That(journalAccount, Is.Not.Null);
                Assert.That(snapshotAccount!.IsLocked, Is.True);
                Assert.That(journalAccount!.IsLocked, Is.False);
            }
        );
    }

    [Test]
    public async Task InitializeAsync_ShouldPreserveAccountTypeAcrossSnapshotAndJournalReplay()
    {
        using var tempDirectory = new TempDirectory();
        var unitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await unitOfWork.InitializeAsync();

        await unitOfWork.Accounts.UpsertAsync(
            new()
            {
                Id = (Serial)0x00000030,
                Username = "snapshot-privileged",
                PasswordHash = "pw",
                AccountType = AccountType.GameMaster
            }
        );

        await unitOfWork.SaveSnapshotAsync();

        await unitOfWork.Accounts.UpsertAsync(
            new()
            {
                Id = (Serial)0x00000031,
                Username = "journal-privileged",
                PasswordHash = "pw",
                AccountType = AccountType.Administrator
            }
        );

        var secondUnitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await secondUnitOfWork.InitializeAsync();

        var snapshotAccount = await secondUnitOfWork.Accounts.GetByUsernameAsync("snapshot-privileged");
        var journalAccount = await secondUnitOfWork.Accounts.GetByUsernameAsync("journal-privileged");

        Assert.Multiple(
            () =>
            {
                Assert.That(snapshotAccount, Is.Not.Null);
                Assert.That(journalAccount, Is.Not.Null);
                Assert.That(snapshotAccount!.AccountType, Is.EqualTo(AccountType.GameMaster));
                Assert.That(journalAccount!.AccountType, Is.EqualTo(AccountType.Administrator));
            }
        );
    }

    [Test]
    public async Task InitializeAsync_ShouldReplayJournalEntriesAfterSnapshot()
    {
        using var tempDirectory = new TempDirectory();
        var unitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await unitOfWork.InitializeAsync();

        await unitOfWork.Accounts.UpsertAsync(
            new()
            {
                Id = (Serial)0x00000003,
                Username = "before-snapshot",
                PasswordHash = "pw"
            }
        );

        await unitOfWork.SaveSnapshotAsync();

        await unitOfWork.Accounts.UpsertAsync(
            new()
            {
                Id = (Serial)0x00000004,
                Username = "after-snapshot",
                PasswordHash = "pw"
            }
        );

        var secondUnitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await secondUnitOfWork.InitializeAsync();

        Assert.That(await secondUnitOfWork.Accounts.GetByUsernameAsync("before-snapshot"), Is.Not.Null);
        Assert.That(await secondUnitOfWork.Accounts.GetByUsernameAsync("after-snapshot"), Is.Not.Null);
    }

    [Test]
    public async Task QueryAsync_OnAccounts_ShouldProjectMatchingResults()
    {
        using var tempDirectory = new TempDirectory();
        var unitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await unitOfWork.InitializeAsync();

        await unitOfWork.Accounts.UpsertAsync(new() { Id = (Serial)0x1001, Username = "alpha", PasswordHash = "pw" });
        await unitOfWork.Accounts.UpsertAsync(new() { Id = (Serial)0x1002, Username = "beta", PasswordHash = "pw" });
        await unitOfWork.Accounts.UpsertAsync(new() { Id = (Serial)0x1003, Username = "admin", PasswordHash = "pw" });

        var usernames = await unitOfWork.Accounts.QueryAsync(
                            account => account.Username.StartsWith('a'),
                            account => account.Username
                        );

        Assert.That(usernames.OrderBy(x => x).ToArray(), Is.EqualTo(ExpectedAccountUsernames));
    }

    [Test]
    public async Task QueryAsync_OnItems_ShouldFilterAndProject()
    {
        using var tempDirectory = new TempDirectory();
        var unitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await unitOfWork.InitializeAsync();

        await unitOfWork.Items.UpsertAsync(new() { Id = (Serial)0x3001, ItemId = 0x0EED });
        await unitOfWork.Items.UpsertAsync(new() { Id = (Serial)0x3002, ItemId = 0x0F3F });

        var coinIds = await unitOfWork.Items.QueryAsync(
                          item => item.ItemId == 0x0EED,
                          item => item.Id
                      );

        Assert.That(coinIds.ToArray(), Is.EqualTo(new[] { (Serial)0x3001 }));
    }

    [Test]
    public async Task QueryAsync_OnMobiles_ShouldReturnOnlyPlayers()
    {
        using var tempDirectory = new TempDirectory();
        var unitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await unitOfWork.InitializeAsync();

        await unitOfWork.Mobiles.UpsertAsync(new() { Id = (Serial)0x2001, IsPlayer = true, IsAlive = true });
        await unitOfWork.Mobiles.UpsertAsync(new() { Id = (Serial)0x2002, IsPlayer = false, IsAlive = true });

        var playerIds = await unitOfWork.Mobiles.QueryAsync(
                            mobile => mobile.IsPlayer,
                            mobile => mobile.Id
                        );

        Assert.That(playerIds.ToArray(), Is.EqualTo(new[] { (Serial)0x2001 }));
    }

    [Test]
    public async Task SaveSnapshotAsync_ShouldPersistAllEntities()
    {
        using var tempDirectory = new TempDirectory();
        var unitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await unitOfWork.InitializeAsync();

        await unitOfWork.Accounts.UpsertAsync(
            new()
            {
                Id = (Serial)0x00000002,
                Username = "snapshot-user",
                PasswordHash = "pw"
            }
        );

        await unitOfWork.Mobiles.UpsertAsync(
            new()
            {
                Id = (Serial)0x00000010,
                AccountId = (Serial)0x00000002,
                Name = "snapshot-mobile",
                MapId = 1,
                Direction = DirectionType.East,
                IsPlayer = true,
                IsAlive = true,
                Gender = GenderType.Female,
                RaceIndex = 1,
                ProfessionId = 2,
                SkinHue = 0x0455,
                HairStyle = 0x0203,
                HairHue = 0x0304,
                FacialHairStyle = 0x0000,
                FacialHairHue = 0x0000,
                Strength = 60,
                Dexterity = 50,
                Intelligence = 40,
                Hits = 60,
                Mana = 40,
                Stamina = 50,
                MaxHits = 60,
                MaxMana = 40,
                MaxStamina = 50,
                Level = 12,
                Experience = 987654,
                SkillPoints = 8,
                StatPoints = 6,
                FireResistance = 15,
                ColdResistance = 11,
                PoisonResistance = 9,
                EnergyResistance = 13,
                Luck = 42,
                BackpackId = (Serial)0x40000020,
                EquippedItemIds = new()
                {
                    [ItemLayerType.Shirt] = (Serial)0x40000021,
                    [ItemLayerType.Pants] = (Serial)0x40000022,
                    [ItemLayerType.Shoes] = (Serial)0x40000023
                },
                IsWarMode = false,
                IsHidden = false,
                IsFrozen = false,
                IsPoisoned = false,
                IsBlessed = false,
                Notoriety = Notoriety.Innocent,
                CreatedUtc = new(2026, 2, 19, 12, 0, 0, DateTimeKind.Utc),
                LastLoginUtc = new(2026, 2, 19, 13, 0, 0, DateTimeKind.Utc),
                Location = new(10, 20, 0)
            }
        );

        await unitOfWork.Items.UpsertAsync(
            new()
            {
                Id = (Serial)0x40000010,
                ItemId = 0x0EED,
                Hue = 0x0481,
                Location = new(10, 20, 0),
                ParentContainerId = (Serial)0x40000020,
                ContainerPosition = new(42, 84),
                EquippedMobileId = (Serial)0x00000010,
                EquippedLayer = ItemLayerType.Shirt
            }
        );

        await unitOfWork.SaveSnapshotAsync();

        var secondUnitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await secondUnitOfWork.InitializeAsync();

        var loadedAccount = await secondUnitOfWork.Accounts.GetByUsernameAsync("snapshot-user");
        var loadedMobile = await secondUnitOfWork.Mobiles.GetByIdAsync((Serial)0x00000010);
        var loadedItem = await secondUnitOfWork.Items.GetByIdAsync((Serial)0x40000010);

        Assert.Multiple(
            () =>
            {
                Assert.That(loadedAccount, Is.Not.Null);
                Assert.That(loadedMobile, Is.Not.Null);
                Assert.That(loadedItem, Is.Not.Null);

                Assert.That(loadedMobile!.AccountId, Is.EqualTo((Serial)0x00000002));
                Assert.That(loadedMobile.Name, Is.EqualTo("snapshot-mobile"));
                Assert.That(loadedMobile.MapId, Is.EqualTo(1));
                Assert.That(loadedMobile.Direction, Is.EqualTo(DirectionType.East));
                Assert.That(loadedMobile.Gender, Is.EqualTo(GenderType.Female));
                Assert.That(loadedMobile.RaceIndex, Is.EqualTo(1));
                Assert.That(loadedMobile.ProfessionId, Is.EqualTo(2));
                Assert.That(loadedMobile.SkinHue, Is.EqualTo(0x0455));
                Assert.That(loadedMobile.HairStyle, Is.EqualTo(0x0203));
                Assert.That(loadedMobile.HairHue, Is.EqualTo(0x0304));
                Assert.That(loadedMobile.Strength, Is.EqualTo(60));
                Assert.That(loadedMobile.Dexterity, Is.EqualTo(50));
                Assert.That(loadedMobile.Intelligence, Is.EqualTo(40));
                Assert.That(loadedMobile.Hits, Is.EqualTo(60));
                Assert.That(loadedMobile.Mana, Is.EqualTo(40));
                Assert.That(loadedMobile.Stamina, Is.EqualTo(50));
                Assert.That(loadedMobile.MaxHits, Is.EqualTo(60));
                Assert.That(loadedMobile.MaxMana, Is.EqualTo(40));
                Assert.That(loadedMobile.MaxStamina, Is.EqualTo(50));
                Assert.That(loadedMobile.Level, Is.EqualTo(12));
                Assert.That(loadedMobile.Experience, Is.EqualTo(987654));
                Assert.That(loadedMobile.SkillPoints, Is.EqualTo(8));
                Assert.That(loadedMobile.StatPoints, Is.EqualTo(6));
                Assert.That(loadedMobile.FireResistance, Is.EqualTo(15));
                Assert.That(loadedMobile.ColdResistance, Is.EqualTo(11));
                Assert.That(loadedMobile.PoisonResistance, Is.EqualTo(9));
                Assert.That(loadedMobile.EnergyResistance, Is.EqualTo(13));
                Assert.That(loadedMobile.Luck, Is.EqualTo(42));
                Assert.That(loadedMobile.BackpackId, Is.EqualTo((Serial)0x40000020));
                Assert.That(loadedMobile.EquippedItemIds[ItemLayerType.Shirt], Is.EqualTo((Serial)0x40000021));
                Assert.That(loadedMobile.EquippedItemIds[ItemLayerType.Pants], Is.EqualTo((Serial)0x40000022));
                Assert.That(loadedMobile.EquippedItemIds[ItemLayerType.Shoes], Is.EqualTo((Serial)0x40000023));
                Assert.That(loadedMobile.Notoriety, Is.EqualTo(Notoriety.Innocent));
                Assert.That(loadedMobile.CreatedUtc, Is.EqualTo(new DateTime(2026, 2, 19, 12, 0, 0, DateTimeKind.Utc)));
                Assert.That(loadedMobile.LastLoginUtc, Is.EqualTo(new DateTime(2026, 2, 19, 13, 0, 0, DateTimeKind.Utc)));
                Assert.That(loadedItem!.ParentContainerId, Is.EqualTo((Serial)0x40000020));
                Assert.That(loadedItem.ContainerPosition.X, Is.EqualTo(42));
                Assert.That(loadedItem.ContainerPosition.Y, Is.EqualTo(84));
                Assert.That(loadedItem.Hue, Is.EqualTo(0x0481));
                Assert.That(loadedItem.EquippedMobileId, Is.EqualTo((Serial)0x00000010));
                Assert.That(loadedItem.EquippedLayer, Is.EqualTo(ItemLayerType.Shirt));
            }
        );
    }

    private static PersistenceUnitOfWork CreateUnitOfWork(string directory)
    {
        var options = new PersistenceOptions(
            Path.Combine(directory, "world.snapshot.bin"),
            Path.Combine(directory, "world.journal.bin")
        );

        return new(options);
    }

    private static async Task WriteAccountsWithRetryAsync(
        PersistenceUnitOfWork unitOfWork,
        int startIndex,
        int count,
        int maxRetries = 20
    )
    {
        for (var i = 0; i < count; i++)
        {
            var currentIndex = startIndex + i;
            var account = new UOAccountEntity
            {
                Id = (Serial)(uint)(0x00050000 + currentIndex),
                Username = $"multi-uow-{currentIndex}",
                PasswordHash = "pw"
            };

            var retries = 0;

            while (true)
            {
                try
                {
                    await unitOfWork.Accounts.UpsertAsync(account);

                    break;
                }
                catch (IOException) when (retries < maxRetries)
                {
                    retries++;
                    await Task.Delay(5);
                }
            }
        }
    }
}
