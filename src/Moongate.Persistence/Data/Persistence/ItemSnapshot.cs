using MemoryPack;

namespace Moongate.Persistence.Data.Persistence;

/// <summary>
/// Serialized item state used inside world snapshots and journal payloads.
/// </summary>
[MemoryPackable]
public sealed partial class ItemSnapshot
{
    public uint Id { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public int Z { get; set; }

    public int ItemId { get; set; }

    public int? GumpId { get; set; }
}
