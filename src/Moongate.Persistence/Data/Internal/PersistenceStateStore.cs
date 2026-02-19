using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.Persistence.Data.Internal;

/// <summary>
/// In-memory mutable world state shared by persistence repositories.
/// </summary>
internal sealed class PersistenceStateStore
{
    public Dictionary<Serial, UOAccountEntity> AccountsById { get; } = [];

    public Dictionary<string, Serial> AccountNameIndex { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<Serial, UOMobileEntity> MobilesById { get; } = [];

    public Dictionary<Serial, UOItemEntity> ItemsById { get; } = [];

    public object SyncRoot { get; } = new();

    public long LastSequenceId { get; set; }

    public uint LastAccountId { get; set; } = (uint)(Serial.MobileStart - 1);

    public uint LastMobileId { get; set; } = (uint)(Serial.MobileStart - 1);

    public uint LastItemId { get; set; } = Serial.ItemOffset - 1;
}
