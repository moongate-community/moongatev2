using System.Collections.Concurrent;
using Moongate.Network.Packets.Interfaces;
using Moongate.Server.Data.Packets;
using Moongate.Server.Interfaces.Services.Packets;

namespace Moongate.Server.Services.Packets;

public sealed class OutgoingPacketQueue : IOutgoingPacketQueue
{
    private readonly ConcurrentQueue<OutgoingGamePacket> _queue = new();

    public void Enqueue(long sessionId, IGameNetworkPacket packet)
    {
        _queue.Enqueue(new(sessionId, packet, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
    }

    public bool TryDequeue(out OutgoingGamePacket gamePacket)
        => _queue.TryDequeue(out gamePacket);
}
