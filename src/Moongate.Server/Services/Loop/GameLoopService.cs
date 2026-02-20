using System.Diagnostics;
using Moongate.Abstractions.Services.Base;
using Moongate.Server.Data.Config;
using Moongate.Server.Data.Metrics;
using Moongate.Server.Interfaces.Services.GameLoop;
using Moongate.Server.Interfaces.Services.Messaging;
using Moongate.Server.Interfaces.Services.Metrics;
using Moongate.Server.Interfaces.Services.Packets;
using Moongate.Server.Interfaces.Services.Timing;
using Serilog;

namespace Moongate.Server.Services.GameLoop;

public class GameLoopService : BaseMoongateService, IGameLoopService, IGameLoopMetricsSource, IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly IMessageBusService _messageBusService;
    private readonly ITimerService _timerService;
    private readonly ILogger _logger = Log.ForContext<GameLoopService>();
    private readonly IPacketDispatchService _packetDispatchService;
    private readonly TimeSpan _tickInterval;
    private readonly Lock _metricsSync = new();

    private Thread? _loopThread;
    private long _tickCount;
    private TimeSpan _uptime;
    private double _averageTickMs;

    public GameLoopService(
        IPacketDispatchService packetDispatchService,
        IMessageBusService messageBusService,
        ITimerService timerService,
        TimerServiceConfig? timerServiceConfig = null
    )
    {
        _packetDispatchService = packetDispatchService;
        _messageBusService = messageBusService;
        _timerService = timerService;
        _tickInterval = timerServiceConfig?.TickDuration ?? TimeSpan.FromMilliseconds(8);

        _logger.Information(
            "GameLoopService initialized with tick interval of {TickInterval} ms",
            _tickInterval.TotalMilliseconds
        );
    }

    public GameLoopMetricsSnapshot GetMetricsSnapshot()
    {
        lock (_metricsSync)
        {
            return new(_tickCount, _uptime, _averageTickMs);
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

            ProcessTick();

            var elapsed = Stopwatch.GetElapsedTime(tickStart);

            lock (_metricsSync)
            {
                _tickCount++;
                _uptime += elapsed;
                _averageTickMs = _averageTickMs * 0.95 + elapsed.TotalMilliseconds * 0.05;
            }

            var remaining = _tickInterval - Stopwatch.GetElapsedTime(tickStart);

            if (remaining.TotalMilliseconds >= 1.0)
            {
                Thread.Sleep((int)remaining.TotalMilliseconds);
            }
        }
    }

    private void ProcessTick()
    {
        DrainPacketQueue();
        _timerService.ProcessTick();
    }

    private void DrainPacketQueue()
    {
        while (_messageBusService.TryReadIncomingPacket(out var gamePacket))
        {
            _packetDispatchService.NotifyPacketListeners(gamePacket);
        }
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
