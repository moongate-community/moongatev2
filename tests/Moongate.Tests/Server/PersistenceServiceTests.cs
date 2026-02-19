using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Data.Config;
using Moongate.Server.Interfaces.Services.Timing;
using Moongate.Server.Services.Persistence;
using Moongate.Server.Services.Timing;
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

        var first = CreatePersistenceService(directories);
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

        var second = CreatePersistenceService(directories);
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
        var service = CreatePersistenceService(directories);

        await service.StartAsync();
        await service.SaveAsync();

        var snapshotPath = Path.Combine(directories[DirectoryType.Save], "world.snapshot.bin");
        Assert.That(File.Exists(snapshotPath), Is.True);
    }

    [Test]
    public async Task SaveAsync_ShouldUpdateSnapshotMetrics()
    {
        using var temp = new TempDirectory();
        var directories = new DirectoriesConfig(temp.Path, Enum.GetNames<DirectoryType>());
        var service = CreatePersistenceService(directories);

        await service.StartAsync();
        await service.SaveAsync();
        var snapshot = service.GetMetricsSnapshot();

        Assert.Multiple(
            () =>
            {
                Assert.That(snapshot.TotalSaves, Is.GreaterThanOrEqualTo(1));
                Assert.That(snapshot.LastSaveDurationMs, Is.GreaterThanOrEqualTo(0));
                Assert.That(snapshot.LastSaveTimestampUtc, Is.Not.Null);
                Assert.That(snapshot.SaveErrors, Is.EqualTo(0));
            }
        );
    }

    [Test]
    public async Task StartAsync_ShouldUseConfiguredSaveInterval()
    {
        using var temp = new TempDirectory();
        var directories = new DirectoriesConfig(temp.Path, Enum.GetNames<DirectoryType>());
        var timerSpy = new TimerServiceSpy();
        var config = new MoongateConfig
        {
            Persistence = new MoongatePersistenceConfig
            {
                SaveIntervalSeconds = 12
            }
        };

        var service = new PersistenceService(directories, timerSpy, config);
        await service.StartAsync();

        Assert.That(timerSpy.LastInterval, Is.EqualTo(TimeSpan.FromSeconds(12)));
    }

    private static PersistenceService CreatePersistenceService(DirectoriesConfig directoriesConfig)
        => new(
            directoriesConfig,
            new TimerWheelService(
                new TimerServiceConfig
                {
                    TickDuration = TimeSpan.FromMilliseconds(250),
                    WheelSize = 512
                }
            ),
            new MoongateConfig()
        );

    private sealed class TimerServiceSpy : ITimerService
    {
        public TimeSpan? LastInterval { get; private set; }

        public void ProcessTick()
        {
        }

        public string RegisterTimer(
            string name,
            TimeSpan interval,
            Action callback,
            TimeSpan? delay = null,
            bool repeat = false
        )
        {
            LastInterval = interval;
            return "timer-spy";
        }

        public string RegisterTimer(
            string name,
            TimeSpan interval,
            Func<CancellationToken, ValueTask> callback,
            TimeSpan? delay = null,
            bool repeat = false
        )
        {
            LastInterval = interval;
            return "timer-spy";
        }

        public void UnregisterAllTimers()
        {
        }

        public bool UnregisterTimer(string timerId) => true;

        public int UnregisterTimersByName(string name) => 0;
    }
}
