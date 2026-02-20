namespace Moongate.Server.Data.Config;

/// <summary>
/// Configuration values used to construct the timer wheel service.
/// </summary>
public sealed class TimerServiceConfig
{
    /// <summary>
    /// Gets or sets the timer-wheel tick duration.
    /// </summary>
    public TimeSpan TickDuration { get; set; } = TimeSpan.FromMilliseconds(8);

    /// <summary>
    /// Gets or sets the timer-wheel slot count.
    /// </summary>
    public int WheelSize { get; set; } = 512;
}
