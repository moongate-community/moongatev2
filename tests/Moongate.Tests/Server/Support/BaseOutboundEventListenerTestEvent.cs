using Moongate.Server.Data.Events;

namespace Moongate.Tests.Server.Support;

public readonly record struct BaseOutboundEventListenerTestEvent(long Value) : IGameEvent;
