using Moongate.Server.Services;

namespace Moongate.Tests.Server;

public class TimerWheelServiceTests
{
    [Test]
    public void ProcessTick_OneShotTimer_ShouldExecuteOnce()
    {
        var service = new TimerWheelService(TimeSpan.FromMilliseconds(100), 64);
        var fired = 0;

        service.RegisterTimer("one-shot", TimeSpan.FromMilliseconds(100), () => fired++);

        service.ProcessTick();
        service.ProcessTick();

        Assert.That(fired, Is.EqualTo(1));
    }

    [Test]
    public void ProcessTick_RepeatingTimer_ShouldExecuteEveryInterval()
    {
        var service = new TimerWheelService(TimeSpan.FromMilliseconds(100), 64);
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
        var service = new TimerWheelService(TimeSpan.FromMilliseconds(100), 64);
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
        var service = new TimerWheelService(TimeSpan.FromMilliseconds(100), 64);
        var fired = 0;

        service.RegisterTimer("same-name", TimeSpan.FromMilliseconds(100), () => fired++);
        service.RegisterTimer("same-name", TimeSpan.FromMilliseconds(100), () => fired++);

        var removed = service.UnregisterTimersByName("same-name");
        service.ProcessTick();

        Assert.That(removed, Is.EqualTo(2));
        Assert.That(fired, Is.EqualTo(0));
    }
}
