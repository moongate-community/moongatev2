using Moongate.Network.Packets.Interfaces;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Services;
using Moongate.Server.Listeners.Base;

namespace Moongate.Tests.Server.Support;

public sealed class BasePacketListenerTestListener : BasePacketListener
{
    public BasePacketListenerTestListener(IOutgoingPacketQueue outgoingPacketQueue)
        : base(outgoingPacketQueue) { }

    public bool Called { get; private set; }

    protected override Task<bool> HandleCoreAsync(GameSession session, IGameNetworkPacket packet)
    {
        Called = true;
        Enqueue(session, packet);
        return Task.FromResult(true);
    }
}
