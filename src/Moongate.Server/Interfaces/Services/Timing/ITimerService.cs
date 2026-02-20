namespace Moongate.Server.Interfaces.Services.Timing;

/// <summary>
/// Provides named timer scheduling using the game-loop tick cadence.
/// </summary>
public interface ITimerService
{
    /// <summary>
    /// Advances the timer wheel by one tick and executes due callbacks.
    /// </summary>
    void ProcessTick();

    /// <summary>
    /// Registers a synchronous timer callback.
    /// </summary>
    /// <param name="name">Logical timer name.</param>
    /// <param name="interval">Interval used as due-time for one-shot or period for repeating timers.</param>
    /// <param name="callback">Callback executed when timer expires.</param>
    /// <param name="delay">Optional initial delay. If null, <paramref name="interval" /> is used.</param>
    /// <param name="repeat">Whether the timer repeats.</param>
    /// <returns>Unique timer identifier.</returns>
    string RegisterTimer(
        string name,
        TimeSpan interval,
        Action callback,
        TimeSpan? delay = null,
        bool repeat = false
    );

    /// <summary>
    /// Removes all registered timers.
    /// </summary>
    void UnregisterAllTimers();

    /// <summary>
    /// Removes a timer by id.
    /// </summary>
    bool UnregisterTimer(string timerId);

    /// <summary>
    /// Removes all timers sharing the provided name.
    /// </summary>
    int UnregisterTimersByName(string name);
}
