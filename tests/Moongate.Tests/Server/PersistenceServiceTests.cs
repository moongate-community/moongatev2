using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Interfaces.Services;
using Moongate.Server.Services;
using Moongate.Tests.TestSupport;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.Tests.Server;

public class PersistenceServiceTests
{
    [Test]
    public async Task StartAsync_AndStopAsync_ShouldPersistDataAcrossRestart()
    {
        using var temp = new TempDirectory();
        var directories = new DirectoriesConfig(temp.Path, Enum.GetNames<DirectoryType>());

        var first = new PersistenceService(directories);
        await first.StartAsync();

        await first.UnitOfWork.Accounts.UpsertAsync(
            new UOAccountEntity
            {
                Id = (Serial)0x00000033,
                Username = "persist-user",
                PasswordHash = "pw"
            }
        );

        await first.StopAsync();

        var second = new PersistenceService(directories);
        await second.StartAsync();

        var loaded = await second.UnitOfWork.Accounts.GetByUsernameAsync("persist-user");

        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.Id, Is.EqualTo((Serial)0x00000033));
    }

    [Test]
    public async Task SaveAsync_ShouldWriteSnapshotFileInSaveDirectory()
    {
        using var temp = new TempDirectory();
        var directories = new DirectoriesConfig(temp.Path, Enum.GetNames<DirectoryType>());
        var service = new PersistenceService(directories);

        await service.StartAsync();
        await service.SaveAsync();

        var snapshotPath = Path.Combine(directories[DirectoryType.Save], "world.snapshot.bin");
        Assert.That(File.Exists(snapshotPath), Is.True);
    }
}
