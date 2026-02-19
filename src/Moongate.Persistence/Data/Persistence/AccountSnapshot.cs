using MemoryPack;

namespace Moongate.Persistence.Data.Persistence;

/// <summary>
/// Serialized account state used inside world snapshots and journal payloads.
/// </summary>
[MemoryPackable]
public sealed partial class AccountSnapshot
{
    public uint Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public long CreatedUtcTicks { get; set; }

    public long LastLoginUtcTicks { get; set; }

    public uint[] CharacterIds { get; set; } = [];
}
