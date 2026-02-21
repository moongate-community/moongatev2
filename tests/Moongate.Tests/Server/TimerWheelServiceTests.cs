using Moongate.Server.Services.Timing;

namespace Moongate.Tests.Server;

public class TimerWheelServiceTests
{
    [Test]
    public void UpdateTicksDelta_ShouldAdvanceTimerWheelUsingTimestampDelta()
    {
        var service = new TimerWheelService(
            new()
            {
                TickDuration = TimeSpan.FromMilliseconds(100),
                WheelSize = 64
            }
        );
        var fired = 0;
        service.RegisterTimer("delta", TimeSpan.FromMilliseconds(100), () => fired++);

        var processed0 = service.UpdateTicksDelta(1_000);
        var processed1 = service.UpdateTicksDelta(1_099);
        var processed2 = service.UpdateTicksDelta(1_100);

        Assert.Multiple(
            () =>
            {
                Assert.That(processed0, Is.EqualTo(0));
                Assert.That(processed1, Is.EqualTo(0));
                Assert.That(processed2, Is.EqualTo(1));
                Assert.That(fired, Is.EqualTo(1));
            }
        );
    }

    [Test]
    public void GetMetricsSnapshot_ShouldExposeExecutionAndErrorCounters()
    {
        var service = new TimerWheelService(
            new()
            {
                TickDuration = TimeSpan.FromMilliseconds(50),
                WheelSize = 64
            }
        );

        service.RegisterTimer("ok", TimeSpan.FromMilliseconds(50), () => { });
        service.RegisterTimer("fail", TimeSpan.FromMilliseconds(50), () => throw new InvalidOperationException("boom"));

        service.ProcessTick();

        var metrics = service.GetMetricsSnapshot();

        Assert.Multiple(
            () =>
            {
                Assert.That(metrics.TotalRegisteredTimers, Is.EqualTo(2));
                Assert.That(metrics.TotalExecutedCallbacks, Is.EqualTo(2));
                Assert.That(metrics.CallbackErrors, Is.EqualTo(1));
                Assert.That(metrics.AverageCallbackDurationMs, Is.GreaterThanOrEqualTo(0));
            }
        );
    }

    [Test]
    public void ProcessTick_OneShotTimer_ShouldExecuteOnce()
    {
        var service = new TimerWheelService(
            new()
            {
                TickDuration = TimeSpan.FromMilliseconds(100),
                WheelSize = 64
            }
        );
        var fired = 0;

        service.RegisterTimer("one-shot", TimeSpan.FromMilliseconds(100), () => fired++);

        service.ProcessTick();
        service.ProcessTick();

        Assert.That(fired, Is.EqualTo(1));
    }

    [Test]
    public void ProcessTick_RepeatingTimer_ShouldExecuteEveryInterval()
    {
        var service = new TimerWheelService(
            new()
            {
                TickDuration = TimeSpan.FromMilliseconds(100),
                WheelSize = 64
            }
        );
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
        var service = new TimerWheelService(
            new()
            {
                TickDuration = TimeSpan.FromMilliseconds(100),
                WheelSize = 64
            }
        );
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
        var service = new TimerWheelService(
            new()
            {
                TickDuration = TimeSpan.FromMilliseconds(100),
                WheelSize = 64
            }
        );
        var fired = 0;

        service.RegisterTimer("same-name", TimeSpan.FromMilliseconds(100), () => fired++);
        service.RegisterTimer("same-name", TimeSpan.FromMilliseconds(100), () => fired++);

        var removed = service.UnregisterTimersByName("same-name");
        service.ProcessTick();

        Assert.That(removed, Is.EqualTo(2));
        Assert.That(fired, Is.EqualTo(0));
    }
}
