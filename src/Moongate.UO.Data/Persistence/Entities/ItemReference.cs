using Moongate.UO.Data.Ids;

namespace Moongate.UO.Data.Persistence.Entities;

/// <summary>
/// Lightweight immutable item snapshot used for runtime equipment projections.
/// </summary>
/// <param name="Id">Item serial.</param>
/// <param name="ItemId">Item graphic identifier.</param>
/// <param name="Hue">Item hue.</param>
public readonly record struct ItemReference(Serial Id, int ItemId, int Hue);
