namespace Moongate.Persistence.Types;

/// <summary>
/// Identifies a journaled persistence operation.
/// </summary>
public enum PersistenceOperationType : byte
{
    UpsertAccount = 1,
    RemoveAccount = 2,
    UpsertMobile = 3,
    RemoveMobile = 4,
    UpsertItem = 5,
    RemoveItem = 6
}
