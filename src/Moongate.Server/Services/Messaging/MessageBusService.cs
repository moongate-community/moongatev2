using System.Threading.Channels;
using Moongate.Server.Data.Packets;
using Moongate.Server.Interfaces.Services.Messaging;
using Serilog;

namespace Moongate.Server.Services.Messaging;

public sealed class MessageBusService : IMessageBusService
{
    private readonly Channel<IncomingGamePacket> _incomingPackets = Channel.CreateUnbounded<IncomingGamePacket>(
        new()
        {
            SingleReader = true,
            SingleWriter = false
        }
    );

    private readonly ILogger _logger = Log.ForContext<MessageBusService>();
    private int _queueDepth;

    public int CurrentQueueDepth => Math.Max(0, Volatile.Read(ref _queueDepth));

    public void PublishIncomingPacket(IncomingGamePacket packet)
    {
        if (!_incomingPackets.Writer.TryWrite(packet))
        {
            _logger.Warning("Failed to publish incoming packet 0x{OpCode:X2}", packet.PacketId);
            return;
        }

        Interlocked.Increment(ref _queueDepth);
    }

    public bool TryReadIncomingPacket(out IncomingGamePacket packet)
    {
        if (!_incomingPackets.Reader.TryRead(out packet))
        {
            return false;
        }

        Interlocked.Decrement(ref _queueDepth);
        return true;
    }
}
