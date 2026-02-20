using System.Threading.Channels;
using Moongate.Network.Packets.Interfaces;
using Moongate.Server.Data.Packets;
using Moongate.Server.Interfaces.Services.Packets;

namespace Moongate.Server.Services.Packets;

public sealed class OutgoingPacketQueue : IOutgoingPacketQueue
{
    private readonly Channel<OutgoingGamePacket> _channel =
        Channel.CreateUnbounded<OutgoingGamePacket>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false }
        );

    public void Enqueue(long sessionId, IGameNetworkPacket packet)
        => _channel.Writer.TryWrite(new(sessionId, packet, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));

    public bool TryDequeue(out OutgoingGamePacket gamePacket)
        => _channel.Reader.TryRead(out gamePacket);

    public Task<bool> WaitToReadAsync(CancellationToken cancellationToken)
        => _channel.Reader.WaitToReadAsync(cancellationToken).AsTask();
}
