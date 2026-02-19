using MemoryPack;

namespace Moongate.Persistence.Data.Persistence;

/// <summary>
/// Serialized mobile state used inside world snapshots and journal payloads.
/// </summary>
[MemoryPackable]
public sealed partial class MobileSnapshot
{
    public uint Id { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public int Z { get; set; }

    public bool IsPlayer { get; set; }

    public bool IsAlive { get; set; }

    public byte Gender { get; set; }
}
