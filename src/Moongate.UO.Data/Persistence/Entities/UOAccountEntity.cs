using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Types;

namespace Moongate.UO.Data.Persistence.Entities;

/// <summary>
/// Represents a persisted account with character ownership metadata.
/// </summary>
public class UOAccountEntity
{
    public Serial Id { get; set; }

    public string Username { get; set; }

    public string PasswordHash { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime LastLoginUtc { get; set; } = DateTime.UtcNow;

    public List<Serial> CharacterIds { get; set; } = [];

    public AccountType AccountType { get; set; } = AccountType.Regular;


    public string Email { get; set; }

    public bool IsLocked { get; set; }
}
