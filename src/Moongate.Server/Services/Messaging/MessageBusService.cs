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

    public void PublishIncomingPacket(IncomingGamePacket packet)
    {
        if (!_incomingPackets.Writer.TryWrite(packet))
        {
            _logger.Warning("Failed to publish incoming packet 0x{OpCode:X2}", packet.PacketId);
        }
    }

    public bool TryReadIncomingPacket(out IncomingGamePacket packet)
        => _incomingPackets.Reader.TryRead(out packet);
}
