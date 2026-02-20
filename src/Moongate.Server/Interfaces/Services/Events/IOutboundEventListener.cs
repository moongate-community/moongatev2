using Moongate.Abstractions.Interfaces.Services.Base;
using Moongate.Server.Data.Events;

namespace Moongate.Server.Interfaces.Services.Events;

/// <summary>
/// Contract for outbound event listeners that react to domain events and emit side-effects (for example outbound packets).
/// </summary>
/// <typeparam name="TEvent">Domain event type handled by this listener.</typeparam>
public interface IOutboundEventListener<in TEvent> : IGameEventListener<TEvent>, IMoongateService
    where TEvent : IGameEvent { }
