using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Services.Events;
using Moongate.Server.Services.Events.Base;

namespace Moongate.Tests.Server.Support;

public sealed class RegisterOutboundEventListenerExtensionTestListener
    : BaseOutboundEventListener<PlayerConnectedEvent>
{
    public RegisterOutboundEventListenerExtensionTestListener(IGameEventBusService gameEventBusService)
        : base(gameEventBusService) { }

    protected override Task HandleCoreAsync(PlayerConnectedEvent gameEvent, CancellationToken cancellationToken)
        => Task.CompletedTask;
}
