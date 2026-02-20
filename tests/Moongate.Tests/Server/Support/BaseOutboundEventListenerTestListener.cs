using Moongate.Server.Services.Events.Base;

namespace Moongate.Tests.Server.Support;

public sealed class BaseOutboundEventListenerTestListener
    : BaseOutboundEventListener<BaseOutboundEventListenerTestEvent>
{
    public List<BaseOutboundEventListenerTestEvent> Received { get; } = [];

    public BaseOutboundEventListenerTestListener(BaseOutboundEventListenerTestGameEventBusService gameEventBusService)
        : base(gameEventBusService) { }

    protected override Task HandleCoreAsync(
        BaseOutboundEventListenerTestEvent gameEvent,
        CancellationToken cancellationToken
    )
    {
        Received.Add(gameEvent);

        return Task.CompletedTask;
    }
}
