using DryIoc;
using Moongate.Abstractions.Extensions;
using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Services.Events;

namespace Moongate.Server.Extensions.Bootstrap;

/// <summary>
/// Registers outbound event listeners as hosted Moongate services with startup priority.
/// </summary>
public static class RegisterOutboundEventListenerExtension
{
    /// <summary>
    /// Registers an outbound event listener implementation for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">Handled event type.</typeparam>
    /// <typeparam name="TListener">Concrete outbound event listener implementation.</typeparam>
    /// <param name="container">DI container.</param>
    /// <param name="priority">Hosted startup priority.</param>
    /// <returns>The same container instance.</returns>
    public static Container RegisterOutboundEventListener<TEvent, TListener>(
        this Container container,
        int priority = 0
    )
        where TEvent : IGameEvent
        where TListener : class, IOutboundEventListener<TEvent>
    {
        ArgumentNullException.ThrowIfNull(container);

        container.RegisterMoongateService<IOutboundEventListener<TEvent>, TListener>(priority);

        return container;
    }
}
