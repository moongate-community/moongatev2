using DryIoc;
using Moongate.Abstractions.Data.Internal;
using Moongate.Server.Data.Events;
using Moongate.Server.Extensions.Bootstrap;
using Moongate.Server.Interfaces.Services.Events;
using Moongate.Server.Services.Events;
using Moongate.Tests.Server.Support;

namespace Moongate.Tests.Server;

public class RegisterOutboundEventListenerExtensionTests
{
    [Test]
    public void RegisterOutboundEventListener_ShouldRegisterServiceAndMetadata()
    {
        var container = new Container();
        container.Register<IGameEventBusService, GameEventBusService>(Reuse.Singleton);

        container.RegisterOutboundEventListener<PlayerConnectedEvent, RegisterOutboundEventListenerExtensionTestListener>(
            170
        );

        var resolved = container.Resolve<IOutboundEventListener<PlayerConnectedEvent>>();
        var registrations = container.Resolve<List<ServiceRegistrationObject>>();
        var registration = registrations.SingleOrDefault(
            x =>
                x.ServiceType == typeof(IOutboundEventListener<PlayerConnectedEvent>) &&
                x.ImplementationType == typeof(RegisterOutboundEventListenerExtensionTestListener)
        );

        Assert.Multiple(
            () =>
            {
                Assert.That(
                    resolved,
                    Is.TypeOf<RegisterOutboundEventListenerExtensionTestListener>()
                );
                Assert.That(registration, Is.Not.Null);
                Assert.That(registration!.Priority, Is.EqualTo(170));
            }
        );
    }
}
