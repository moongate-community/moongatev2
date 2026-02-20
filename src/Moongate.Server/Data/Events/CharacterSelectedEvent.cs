using Moongate.UO.Data.Ids;

namespace Moongate.Server.Data.Events;

public readonly record struct CharacterSelectedEvent(
    long Sessionid,
    Serial CharacterId,
    long Timestamp
) : IGameEvent;


