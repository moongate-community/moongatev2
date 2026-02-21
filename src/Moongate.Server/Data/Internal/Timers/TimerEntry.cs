namespace Moongate.Server.Data.Internal.Timers;

internal sealed class TimerEntry
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required Action Callback { get; init; }

    public required TimeSpan Interval { get; init; }

    public required bool Repeat { get; init; }

    public int SlotIndex { get; set; }

    public long RemainingRounds { get; set; }

    public LinkedListNode<TimerEntry>? Node { get; set; }

    public bool Cancelled { get; set; }
}
