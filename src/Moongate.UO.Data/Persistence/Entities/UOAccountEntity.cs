using Moongate.UO.Data.Ids;

namespace Moongate.UO.Data.Persistence.Entities;

/// <summary>
/// Represents a persisted account with character ownership metadata.
/// </summary>
public class UOAccountEntity
{
    public Serial Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime LastLoginUtc { get; set; } = DateTime.UtcNow;

    public List<Serial> CharacterIds { get; set; } = [];
}
