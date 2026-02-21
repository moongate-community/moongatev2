using System.Collections.Concurrent;
using Moongate.Network.Packets.Interfaces;
using Moongate.Server.Data.Packets;
using Moongate.Server.Interfaces.Services.Packets;

namespace Moongate.Server.Services.Packets;

public sealed class OutgoingPacketQueue : IOutgoingPacketQueue
{
    private readonly ConcurrentQueue<OutgoingGamePacket> _queue = new();
    private int _queueDepth;

    public int CurrentQueueDepth => Math.Max(0, Volatile.Read(ref _queueDepth));

    public void Enqueue(long sessionId, IGameNetworkPacket packet)
    {
        _queue.Enqueue(new(sessionId, packet, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
        Interlocked.Increment(ref _queueDepth);
    }

    public bool TryDequeue(out OutgoingGamePacket gamePacket)
    {
        if (!_queue.TryDequeue(out gamePacket))
        {
            return false;
        }

        Interlocked.Decrement(ref _queueDepth);
        return true;
    }
}
