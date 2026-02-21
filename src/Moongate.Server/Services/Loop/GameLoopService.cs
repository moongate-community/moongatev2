using System.Diagnostics;
using Moongate.Abstractions.Services.Base;
using Moongate.Server.Data.Config;
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
    private static readonly bool UseFastTimestampMath = Stopwatch.Frequency % 1000 == 0;
    private static readonly ulong FrequencyInMilliseconds = (ulong)Stopwatch.Frequency / 1000;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly IMessageBusService _messageBusService;
    private readonly IOutgoingPacketQueue _outgoingPacketQueue;
    private readonly IGameNetworkSessionService _gameNetworkSessionService;
    private readonly ITimerService _timerService;
    private readonly IOutboundPacketSender _outboundPacketSender;
    private readonly ILogger _logger = Log.ForContext<GameLoopService>();
    private readonly IPacketDispatchService _packetDispatchService;
    private readonly bool _idleCpuEnabled;
    private readonly int _idleSleepMilliseconds;
    private readonly Lock _metricsSync = new();

    private Thread? _loopThread;
    private long _tickCount;
    private TimeSpan _uptime;
    private double _averageTickMs;
    private double _maxTickMs;
    private long _idleSleepCount;
    private long _totalWorkUnits;
    private double _averageWorkUnits;
    private long _outboundPacketsTotal;

    public GameLoopService(
        IPacketDispatchService packetDispatchService,
        IMessageBusService messageBusService,
        IOutgoingPacketQueue outgoingPacketQueue,
        IGameNetworkSessionService gameNetworkSessionService,
        ITimerService timerService,
        IOutboundPacketSender outboundPacketSender,
        TimerServiceConfig? timerServiceConfig = null
    )
    {
        _packetDispatchService = packetDispatchService;
        _messageBusService = messageBusService;
        _outgoingPacketQueue = outgoingPacketQueue;
        _gameNetworkSessionService = gameNetworkSessionService;
        _timerService = timerService;
        _outboundPacketSender = outboundPacketSender;
        _idleCpuEnabled = timerServiceConfig?.IdleCpuEnabled ?? true;
        _idleSleepMilliseconds = Math.Max(1, timerServiceConfig?.IdleSleepMilliseconds ?? 1);

        _logger.Information(
            "GameLoopService initialized. IdleCpu={IdleCpuEnabled} IdleSleepMs={IdleSleepMilliseconds}",
            _idleCpuEnabled,
            _idleSleepMilliseconds
        );
    }

    public GameLoopMetricsSnapshot GetMetricsSnapshot()
    {
        lock (_metricsSync)
        {
            return new(
                _tickCount,
                _uptime,
                _averageTickMs,
                _maxTickMs,
                _idleSleepCount,
                _averageWorkUnits,
                _outgoingPacketQueue.CurrentQueueDepth,
                _outboundPacketsTotal
            );
        }
    }

    public override Task StartAsync()
    {
        _loopThread = new Thread(RunLoop)
        {
            IsBackground = true,
            Name = "GameLoop"
        };
        _loopThread.Start();

        return Task.CompletedTask;
    }

    public override Task StopAsync()
    {
        if (!_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }

        return Task.CompletedTask;
    }

    private void RunLoop()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            var tickStart = Stopwatch.GetTimestamp();
            var timestampMilliseconds = GetTimestampMilliseconds();

            var workUnits = ProcessTick(timestampMilliseconds);

            var elapsed = Stopwatch.GetElapsedTime(tickStart);

            lock (_metricsSync)
            {
                _tickCount++;
                _uptime += elapsed;
                _averageTickMs = _averageTickMs * 0.95 + elapsed.TotalMilliseconds * 0.05;
                _maxTickMs = Math.Max(_maxTickMs, elapsed.TotalMilliseconds);
                _totalWorkUnits += workUnits;
                _averageWorkUnits = _tickCount == 0 ? 0 : (double)_totalWorkUnits / _tickCount;
            }

            if (_idleCpuEnabled && workUnits == 0)
            {
                Thread.Sleep(_idleSleepMilliseconds);
                Interlocked.Increment(ref _idleSleepCount);
            }
        }
    }

    private int ProcessTick(long timestampMilliseconds)
    {
        var inbound = DrainPacketQueue();
        var timerTicks = _timerService.UpdateTicksDelta(timestampMilliseconds);
        var outbound = DrainOutgoingPacketQueue();

        return inbound + timerTicks + outbound;
    }

    private int DrainPacketQueue()
    {
        var drained = 0;

        while (_messageBusService.TryReadIncomingPacket(out var gamePacket))
        {
            _packetDispatchService.NotifyPacketListeners(gamePacket);
            drained++;
        }

        return drained;
    }

    private int DrainOutgoingPacketQueue()
    {
        var drained = 0;

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

            _outboundPacketSender.Send(client, outgoingPacket);
            drained++;
            Interlocked.Increment(ref _outboundPacketsTotal);
        }

        return drained;
    }

    private static long GetTimestampMilliseconds()
    {
        if (UseFastTimestampMath)
            return (long)((ulong)Stopwatch.GetTimestamp() / FrequencyInMilliseconds);

        return (long)((UInt128)Stopwatch.GetTimestamp() * 1000 / (ulong)Stopwatch.Frequency);
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
}
