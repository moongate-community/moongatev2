using Moongate.Persistence.Data.Persistence;
using Moongate.Persistence.Services.Persistence;
using Moongate.Tests.TestSupport;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;

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
                IsPlayer = true,
                IsAlive = true,
                Location = new(10, 20, 0)
            }
        );

        await unitOfWork.Items.UpsertAsync(
            new()
            {
                Id = (Serial)0x40000010,
                ItemId = 0x0EED,
                Location = new(10, 20, 0)
            }
        );

        await unitOfWork.SaveSnapshotAsync();

        var secondUnitOfWork = CreateUnitOfWork(tempDirectory.Path);
        await secondUnitOfWork.InitializeAsync();

        Assert.That(await secondUnitOfWork.Accounts.GetByUsernameAsync("snapshot-user"), Is.Not.Null);
        Assert.That(await secondUnitOfWork.Mobiles.GetByIdAsync((Serial)0x00000010), Is.Not.Null);
        Assert.That(await secondUnitOfWork.Items.GetByIdAsync((Serial)0x40000010), Is.Not.Null);
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
