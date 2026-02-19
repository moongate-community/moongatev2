using Moongate.Server.Services;
using Moongate.Server.Data.Config;
using Moongate.Server.Interfaces.Services.Metrics;
using Moongate.Server.Services.Timing;

namespace Moongate.Tests.Server;

public class TimerWheelServiceTests
{
    [Test]
    public void ProcessTick_OneShotTimer_ShouldExecuteOnce()
    {
        var service = new TimerWheelService(new TimerServiceConfig
        {
            TickDuration = TimeSpan.FromMilliseconds(100),
            WheelSize = 64
        });
        var fired = 0;

        service.RegisterTimer("one-shot", TimeSpan.FromMilliseconds(100), () => fired++);

        service.ProcessTick();
        service.ProcessTick();

        Assert.That(fired, Is.EqualTo(1));
    }

    [Test]
    public void ProcessTick_RepeatingTimer_ShouldExecuteEveryInterval()
    {
        var service = new TimerWheelService(new TimerServiceConfig
        {
            TickDuration = TimeSpan.FromMilliseconds(100),
            WheelSize = 64
        });
        var fired = 0;

        service.RegisterTimer("repeat", TimeSpan.FromMilliseconds(200), () => fired++, repeat: true);

        service.ProcessTick();
        service.ProcessTick();
        service.ProcessTick();
        service.ProcessTick();
        service.ProcessTick();

        Assert.That(fired, Is.EqualTo(2));
    }

    [Test]
    public void ProcessTick_TimerWithDelay_ShouldWaitBeforeFirstExecution()
    {
        var service = new TimerWheelService(new TimerServiceConfig
        {
            TickDuration = TimeSpan.FromMilliseconds(100),
            WheelSize = 64
        });
        var fired = 0;

        service.RegisterTimer(
            "delayed",
            TimeSpan.FromMilliseconds(100),
            () => fired++,
            TimeSpan.FromMilliseconds(300)
        );

        service.ProcessTick();
        service.ProcessTick();
        Assert.That(fired, Is.EqualTo(0));

        service.ProcessTick();
        Assert.That(fired, Is.EqualTo(1));
    }

    [Test]
    public void UnregisterTimersByName_ShouldRemoveAllMatchingTimers()
    {
        var service = new TimerWheelService(new TimerServiceConfig
        {
            TickDuration = TimeSpan.FromMilliseconds(100),
            WheelSize = 64
        });
        var fired = 0;

        service.RegisterTimer("same-name", TimeSpan.FromMilliseconds(100), () => fired++);
        service.RegisterTimer("same-name", TimeSpan.FromMilliseconds(100), () => fired++);

        var removed = service.UnregisterTimersByName("same-name");
        service.ProcessTick();

        Assert.That(removed, Is.EqualTo(2));
        Assert.That(fired, Is.EqualTo(0));
    }

    [Test]
    public async Task RegisterTimer_AsyncCallback_ShouldExecuteWithoutOverlap()
    {
        var service = new TimerWheelService(
            new TimerServiceConfig
            {
                TickDuration = TimeSpan.FromMilliseconds(50),
                WheelSize = 64
            }
        );
        var fired = 0;

        service.RegisterTimer(
            "repeat-async",
            TimeSpan.FromMilliseconds(50),
            async ct =>
            {
                Interlocked.Increment(ref fired);
                await Task.Delay(200, ct);
            },
            repeat: true
        );

        for (var i = 0; i < 8; i++)
        {
            service.ProcessTick();
            await Task.Delay(10);
        }

        var firstBurst = Volatile.Read(ref fired);

        await Task.Delay(250);
        service.ProcessTick();
        await Task.Delay(30);

        var secondBurst = Volatile.Read(ref fired);

        Assert.That(firstBurst, Is.EqualTo(1));
        Assert.That(secondBurst, Is.EqualTo(2));
    }

    [Test]
    public async Task GetMetricsSnapshot_ShouldExposeExecutionAndErrorCounters()
    {
        var service = new TimerWheelService(
            new TimerServiceConfig
            {
                TickDuration = TimeSpan.FromMilliseconds(50),
                WheelSize = 64
            }
        );

        service.RegisterTimer("ok", TimeSpan.FromMilliseconds(50), () => { });
        service.RegisterTimer("fail", TimeSpan.FromMilliseconds(50), () => throw new InvalidOperationException("boom"));

        service.ProcessTick();
        await Task.Delay(50);

        var metrics = ((ITimerMetricsSource)service).GetMetricsSnapshot();

        Assert.Multiple(
            () =>
            {
                Assert.That(metrics.TotalRegisteredTimers, Is.EqualTo(2));
                Assert.That(metrics.TotalExecutedCallbacks, Is.EqualTo(1));
                Assert.That(metrics.CallbackErrors, Is.EqualTo(1));
                Assert.That(metrics.AverageCallbackDurationMs, Is.GreaterThanOrEqualTo(0));
            }
        );
    }
}
