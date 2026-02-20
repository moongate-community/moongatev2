using System.Diagnostics;
using Moongate.Server.Data.Config;
using Moongate.Server.Data.Internal.Timers;
using Moongate.Server.Data.Metrics;
using Moongate.Server.Interfaces.Services.Metrics;
using Moongate.Server.Interfaces.Services.Timing;
using Serilog;

namespace Moongate.Server.Services.Timing;

/// <summary>
/// Hashed timer-wheel implementation driven by game-loop ticks.
/// </summary>
public sealed class TimerWheelService
    : ITimerService
      , ITimerMetricsSource
{
    private readonly ILogger _logger = Log.ForContext<TimerWheelService>();
    private readonly TimeSpan _tickDuration;
    private readonly LinkedList<TimerEntry>[] _wheel;
    private readonly Lock _syncRoot = new();
    private readonly Dictionary<string, TimerEntry> _timersById = new(StringComparer.Ordinal);
    private readonly Dictionary<string, HashSet<string>> _timerIdsByName = new(StringComparer.Ordinal);

    private long _currentTick;
    private long _totalRegisteredTimers;
    private long _totalExecutedCallbacks;
    private long _callbackErrors;
    private long _totalCallbackElapsedTicks;

    public TimerWheelService(TimerServiceConfig config)
    {
        _tickDuration = config.TickDuration;
        var wheelSize = config.WheelSize;

        if (_tickDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(_tickDuration), "Tick duration must be positive.");

        if (wheelSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(wheelSize), "Wheel size must be positive.");

        _wheel = new LinkedList<TimerEntry>[wheelSize];

        for (var i = 0; i < _wheel.Length; i++)
            _wheel[i] = new();
    }

    public TimerMetricsSnapshot GetMetricsSnapshot()
    {
        int activeTimerCount;

        lock (_syncRoot)
        {
            activeTimerCount = _timersById.Count;
        }

        var executedCallbacks = Interlocked.Read(ref _totalExecutedCallbacks);
        var totalElapsedTicks = Interlocked.Read(ref _totalCallbackElapsedTicks);
        var averageCallbackDurationMs =
            executedCallbacks == 0
                ? 0
                : TimeSpan.FromTicks(totalElapsedTicks / executedCallbacks).TotalMilliseconds;

        return new(
            activeTimerCount,
            Interlocked.Read(ref _totalRegisteredTimers),
            executedCallbacks,
            Interlocked.Read(ref _callbackErrors),
            averageCallbackDurationMs
        );
    }

    public void ProcessTick()
    {
        List<TimerEntry> dueEntries = [];

        lock (_syncRoot)
        {
            _currentTick++;
            var slotIndex = (int)(_currentTick % _wheel.Length);
            var bucket = _wheel[slotIndex];
            var node = bucket.First;

            while (node is not null)
            {
                var next = node.Next;
                var entry = node.Value;

                if (entry.Cancelled)
                {
                    bucket.Remove(node);
                    entry.Node = null;
                    node = next;
                    continue;
                }

                if (entry.RemainingRounds > 0)
                {
                    entry.RemainingRounds--;
                    node = next;
                    continue;
                }

                bucket.Remove(node);
                entry.Node = null;
                dueEntries.Add(entry);

                if (!entry.Repeat)
                    RemoveFromIndexes(entry);

                node = next;
            }
        }

        foreach (var entry in dueEntries)
            ExecuteEntry(entry);
    }

    public string RegisterTimer(
        string name,
        TimeSpan interval,
        Action callback,
        TimeSpan? delay = null,
        bool repeat = false
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Timer name cannot be empty.", nameof(name));

        if (interval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be positive.");

        ArgumentNullException.ThrowIfNull(callback);

        var dueTime = delay ?? interval;

        if (dueTime <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(delay), "Delay must be positive.");

        var entry = new TimerEntry
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = name,
            Callback = callback,
            Interval = interval,
            Repeat = repeat
        };

        lock (_syncRoot)
        {
            _timersById[entry.Id] = entry;
            _totalRegisteredTimers++;

            if (!_timerIdsByName.TryGetValue(name, out var ids))
            {
                ids = [];
                _timerIdsByName[name] = ids;
            }

            ids.Add(entry.Id);
            ScheduleEntry(entry, dueTime);
        }

        return entry.Id;
    }

    public void UnregisterAllTimers()
    {
        lock (_syncRoot)
        {
            _timersById.Clear();
            _timerIdsByName.Clear();

            foreach (var bucket in _wheel)
                bucket.Clear();
        }
    }

    public bool UnregisterTimer(string timerId)
    {
        if (string.IsNullOrWhiteSpace(timerId))
            return false;

        lock (_syncRoot)
        {
            return RemoveEntryById(timerId);
        }
    }

    public int UnregisterTimersByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return 0;

        lock (_syncRoot)
        {
            if (!_timerIdsByName.TryGetValue(name, out var ids) || ids.Count == 0)
                return 0;

            var timerIds = ids.ToArray();
            var removed = 0;

            foreach (var timerId in timerIds)
            {
                if (RemoveEntryById(timerId))
                    removed++;
            }

            return removed;
        }
    }

    private void ExecuteEntry(TimerEntry entry)
    {
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            _logger.Debug(
                "Executing timer callback {TimerName} ({TimerId}) [Repeat={Repeat}]",
                entry.Name,
                entry.Id,
                entry.Repeat
            );

            entry.Callback();
            Interlocked.Increment(ref _totalExecutedCallbacks);
            Interlocked.Add(ref _totalCallbackElapsedTicks, Stopwatch.GetTimestamp() - startedAt);
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _callbackErrors);
            _logger.Error(ex, "Timer callback failed for timer '{TimerName}' ({TimerId}).", entry.Name, entry.Id);
        }

        if (!entry.Repeat)
            return;

        lock (_syncRoot)
        {
            if (!entry.Cancelled && _timersById.ContainsKey(entry.Id))
                ScheduleEntry(entry, entry.Interval);
        }
    }

    private bool RemoveEntryById(string timerId)
    {
        if (!_timersById.TryGetValue(timerId, out var entry))
            return false;

        entry.Cancelled = true;

        if (entry.Node is not null)
        {
            _wheel[entry.SlotIndex].Remove(entry.Node);
            entry.Node = null;
        }

        RemoveFromIndexes(entry);

        return true;
    }

    private void RemoveFromIndexes(TimerEntry entry)
    {
        _timersById.Remove(entry.Id);

        if (!_timerIdsByName.TryGetValue(entry.Name, out var ids))
            return;

        ids.Remove(entry.Id);

        if (ids.Count == 0)
            _timerIdsByName.Remove(entry.Name);
    }

    private void ScheduleEntry(TimerEntry entry, TimeSpan dueTime)
    {
        var ticks = ToWheelTicks(dueTime);
        var targetTick = _currentTick + ticks;
        var slotIndex = (int)(targetTick % _wheel.Length);
        var rounds = (ticks - 1) / _wheel.Length;

        entry.SlotIndex = slotIndex;
        entry.RemainingRounds = rounds;
        entry.Cancelled = false;
        entry.Node = _wheel[slotIndex].AddLast(entry);
    }

    private long ToWheelTicks(TimeSpan dueTime)
    {
        var ticks = (long)Math.Ceiling(dueTime.TotalMilliseconds / _tickDuration.TotalMilliseconds);
        return Math.Max(1, ticks);
    }
}
