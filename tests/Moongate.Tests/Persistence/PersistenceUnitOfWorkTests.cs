using Moongate.Persistence.Data.Persistence;
using Moongate.Persistence.Services.Persistence;
using Moongate.Tests.TestSupport;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.Tests.Persistence;

public class PersistenceUnitOfWorkTests
{
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

    private static PersistenceUnitOfWork CreateUnitOfWork(string directory)
    {
        var options = new PersistenceOptions(
            Path.Combine(directory, "world.snapshot.bin"),
            Path.Combine(directory, "world.journal.bin")
        );

        return new(options);
    }
}
