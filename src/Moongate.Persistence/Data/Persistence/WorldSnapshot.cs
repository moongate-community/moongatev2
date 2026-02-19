using MemoryPack;

namespace Moongate.Persistence.Data.Persistence;

/// <summary>
/// Full persisted world state stored periodically on disk.
/// </summary>
[MemoryPackable]
public sealed partial class WorldSnapshot
{
    public int Version { get; set; } = 1;

    public long CreatedUnixMilliseconds { get; set; }

    public long LastSequenceId { get; set; }

    public AccountSnapshot[] Accounts { get; set; } = [];

    public MobileSnapshot[] Mobiles { get; set; } = [];

    public ItemSnapshot[] Items { get; set; } = [];
}
