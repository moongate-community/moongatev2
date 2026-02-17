using Moongate.UO.Data.Ids;

namespace Moongate.UO.Data.Interfaces.Entities;

/// <summary>
/// Base contract for all UO entities identified by a serial.
/// </summary>
public interface IUOEntity
{
    /// <summary>
    /// Gets the unique serial identifier.
    /// </summary>
    Serial Id { get; }
}
