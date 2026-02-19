using Moongate.UO.Data.Ids;

namespace Moongate.Server.Data.Events;

public readonly record struct CharacterCreatedEvent(
    string CharacterName,
    Serial AccountId,
    Serial CharacterId,
    long Timestamp
) : IGameEvent;
