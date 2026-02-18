using Moongate.Network.Packets.Interfaces;
using Moongate.Server.Data.Packets;
using Moongate.Server.Interfaces.Services;

namespace Moongate.Tests.Server.Support;

public sealed class BasePacketListenerTestOutgoingPacketQueue : IOutgoingPacketQueue
{
    private readonly Queue<OutgoingGamePacket> _items = new();

    public void Enqueue(long sessionId, IGameNetworkPacket packet)
        => _items.Enqueue(new(sessionId, packet, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));

    public bool TryDequeue(out OutgoingGamePacket gamePacket)
    {
        if (_items.Count == 0)
        {
            gamePacket = default;
            return false;
        }

        gamePacket = _items.Dequeue();
        return true;
    }
}
