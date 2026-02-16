using Moongate.Server.Services;

namespace Moongate.Tests.Server;

public class GameLoopServiceTests
{
    private GameLoopService? _service;

    [Test]
    public void Ctor_ShouldStartWithZeroedMetrics()
    {
        _service = new GameLoopService();

        Assert.Multiple(
            () =>
            {
                Assert.That(_service.TickCount, Is.Zero);
                Assert.That(_service.Uptime, Is.EqualTo(TimeSpan.Zero));
                Assert.That(_service.AverageTickMs, Is.Zero);
            }
        );
    }

    [Test]
    public async Task StartAsync_ShouldAdvanceLoopAndUpdateMetrics()
    {
        _service = new GameLoopService();

        await _service.StartAsync();

        var tickAdvanced = await WaitUntilAsync(() => _service.TickCount > 0, TimeSpan.FromSeconds(2));
        var uptimeAdvanced = await WaitUntilAsync(() => _service.Uptime > TimeSpan.Zero, TimeSpan.FromSeconds(2));

        Assert.Multiple(
            () =>
            {
                Assert.That(tickAdvanced, Is.True, "TickCount did not increase in time.");
                Assert.That(uptimeAdvanced, Is.True, "Uptime did not increase in time.");
                Assert.That(_service.AverageTickMs, Is.GreaterThanOrEqualTo(0d));
            }
        );
    }

    [Test]
    public async Task StopAsync_ShouldStopAdvancingTickCount()
    {
        _service = new GameLoopService();
        await _service.StartAsync();

        var tickAdvanced = await WaitUntilAsync(() => _service.TickCount > 0, TimeSpan.FromSeconds(2));
        Assert.That(tickAdvanced, Is.True, "TickCount did not increase in time.");

        await _service.StopAsync();

        var tickAfterStop = _service.TickCount;
        await Task.Delay(500);

        Assert.That(_service.TickCount, Is.LessThanOrEqualTo(tickAfterStop + 1));
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_service is not null)
        {
            await _service.StopAsync();
            _service.Dispose();
        }

        _service = null;
    }

    private static async Task<bool> WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
    {
        var start = DateTime.UtcNow;

        while (DateTime.UtcNow - start < timeout)
        {
            if (condition())
            {
                return true;
            }

            await Task.Delay(20);
        }

        return condition();
    }
}
