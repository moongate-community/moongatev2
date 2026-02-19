using System.Diagnostics;
using Moongate.Abstractions.Services.Base;
using Moongate.Server.Interfaces.Services.Loop;
using Moongate.Server.Interfaces.Services.Messaging;
using Moongate.Server.Interfaces.Services.Packets;
using Moongate.Server.Interfaces.Services.Sessions;
using Moongate.Server.Interfaces.Services.Timing;
using Serilog;

namespace Moongate.Server.Services.Loop;

public class GameLoopService : BaseMoongateService, IGameLoopService, IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly IMessageBusService _messageBusService;
    private readonly IOutgoingPacketQueue _outgoingPacketQueue;
    private readonly IGameNetworkSessionService _gameNetworkSessionService;
    private readonly ITimerService _timerService;
    private readonly IOutboundPacketSender _outboundPacketSender;
    private readonly ILogger _logger = Log.ForContext<GameLoopService>();
    private readonly IPacketDispatchService _packetDispatchService;
    private readonly TimeSpan _tickInterval = TimeSpan.FromMilliseconds(150);

    public long TickCount { get; private set; }
    public TimeSpan Uptime { get; private set; }
    public double AverageTickMs { get; private set; }

    public GameLoopService(
        IPacketDispatchService packetDispatchService,
        IMessageBusService messageBusService,
        IOutgoingPacketQueue outgoingPacketQueue,
        IGameNetworkSessionService gameNetworkSessionService,
        ITimerService timerService,
        IOutboundPacketSender outboundPacketSender
    )
    {
        _packetDispatchService = packetDispatchService;
        _messageBusService = messageBusService;
        _outgoingPacketQueue = outgoingPacketQueue;
        _gameNetworkSessionService = gameNetworkSessionService;
        _timerService = timerService;
        _outboundPacketSender = outboundPacketSender;

        _logger.Information(
            "GameLoopService initialized with tick interval of {TickInterval} ms",
            _tickInterval.TotalMilliseconds
        );
    }

    public void Dispose()
    {
        if (!_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }

        _cancellationTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task StartAsync()
    {
        Task.Run(
            async () =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    var tickStart = Stopwatch.GetTimestamp();

                    ProcessQueue();

                    TickCount++;

                    var elapsed = Stopwatch.GetElapsedTime(tickStart);
                    Uptime += elapsed;
                    AverageTickMs = AverageTickMs * 0.95 + elapsed.TotalMilliseconds * 0.05;

                    var remaining = _tickInterval - elapsed;

                    if (remaining > TimeSpan.Zero)
                    {
                        await Task.Delay(remaining, _cancellationTokenSource.Token);
                    }
                }
            }
        );
    }

    public async Task StopAsync()
    {
        if (!_cancellationTokenSource.IsCancellationRequested)
        {
            await _cancellationTokenSource.CancelAsync();
        }
    }

    private void DrainOutgoingPacketQueue()
    {
        while (_outgoingPacketQueue.TryDequeue(out var outgoingPacket))
        {
            if (
                !_gameNetworkSessionService.TryGet(outgoingPacket.SessionId, out var session) ||
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

            _ = _outboundPacketSender.SendAsync(client, outgoingPacket, _cancellationTokenSource.Token);
        }
    }

    private void DrainPacketQueue()
    {
        while (_messageBusService.TryReadIncomingPacket(out var gamePacket))
        {
            _packetDispatchService.NotifyPacketListeners(gamePacket);
        }
    }

    private void ProcessQueue()
    {
        DrainPacketQueue();
        _timerService.ProcessTick();
        DrainOutgoingPacketQueue();
    }
}
