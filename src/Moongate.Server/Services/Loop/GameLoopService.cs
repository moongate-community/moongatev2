using System.Diagnostics;
using Moongate.Abstractions.Services.Base;
using Moongate.Server.Data.Metrics;
using Moongate.Server.Interfaces.Services.GameLoop;
using Moongate.Server.Interfaces.Services.Messaging;
using Moongate.Server.Interfaces.Services.Metrics;
using Moongate.Server.Interfaces.Services.Packets;
using Moongate.Server.Interfaces.Services.Sessions;
using Moongate.Server.Interfaces.Services.Timing;
using Serilog;

namespace Moongate.Server.Services.GameLoop;

public class GameLoopService : BaseMoongateService, IGameLoopService, IGameLoopMetricsSource, IDisposable
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
    private readonly Lock _metricsSync = new();
    private long _tickCount;
    private TimeSpan _uptime;
    private double _averageTickMs;

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

    public GameLoopMetricsSnapshot GetMetricsSnapshot()
    {
        lock (_metricsSync)
        {
            return new(_tickCount, _uptime, _averageTickMs);
        }
    }

    public async Task StartAsync()
    {
        Task.Run(
            async () =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    var tickStart = Stopwatch.GetTimestamp();

                    await ProcessQueueAsync();

                    var elapsed = Stopwatch.GetElapsedTime(tickStart);

                    lock (_metricsSync)
                    {
                        _tickCount++;
                        _uptime += elapsed;
                        _averageTickMs = _averageTickMs * 0.95 + elapsed.TotalMilliseconds * 0.05;
                    }

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

    private async Task DrainOutgoingPacketQueueAsync()
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

            await _outboundPacketSender.SendAsync(client, outgoingPacket, _cancellationTokenSource.Token);
        }
    }

    private void DrainPacketQueue()
    {
        while (_messageBusService.TryReadIncomingPacket(out var gamePacket))
        {
            _packetDispatchService.NotifyPacketListeners(gamePacket);
        }
    }

    private async Task ProcessQueueAsync()
    {
        DrainPacketQueue();
        _timerService.ProcessTick();
        await DrainOutgoingPacketQueueAsync();
    }
}
