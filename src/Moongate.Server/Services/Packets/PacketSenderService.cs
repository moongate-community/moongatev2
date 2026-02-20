using Moongate.Abstractions.Services.Base;
using Moongate.Server.Interfaces.Services.Packets;
using Moongate.Server.Interfaces.Services.Sessions;
using Serilog;

namespace Moongate.Server.Services.Packets;

public sealed class PacketSenderService : BaseMoongateService, IPacketSenderService, IDisposable
{
    private readonly IOutgoingPacketQueue _queue;
    private readonly IOutboundPacketSender _sender;
    private readonly IGameNetworkSessionService _sessions;
    private readonly ILogger _logger = Log.ForContext<PacketSenderService>();
    private readonly CancellationTokenSource _cts = new();

    private Thread? _thread;

    public PacketSenderService(
        IOutgoingPacketQueue queue,
        IOutboundPacketSender sender,
        IGameNetworkSessionService sessions
    )
    {
        _queue = queue;
        _sender = sender;
        _sessions = sessions;
    }

    public override Task StartAsync()
    {
        _thread = new Thread(RunLoop)
        {
            IsBackground = true,
            Name = "PacketSender"
        };
        _thread.Start();

        return Task.CompletedTask;
    }

    public override Task StopAsync()
    {
        if (!_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }

        return Task.CompletedTask;
    }

    private void RunLoop()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                if (!_queue.WaitToReadAsync(_cts.Token).GetAwaiter().GetResult())
                {
                    break;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }

            while (_queue.TryDequeue(out var outgoingPacket))
            {
                if (
                    !_sessions.TryGet(outgoingPacket.SessionId, out var session) ||
                    session.NetworkSession.Client is not { } client
                )
                {
                    _logger.Warning(
                        "Skipping outbound packet 0x{OpCode:X2}: session {SessionId} is not connected.",
                        outgoingPacket.Packet.OpCode,
                        outgoingPacket.SessionId
                    );

                    continue;
                }

                _sender.Send(client, outgoingPacket);
            }
        }
    }

    public void Dispose()
    {
        if (!_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }

        _cts.Dispose();
        GC.SuppressFinalize(this);
    }
}
