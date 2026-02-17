using System.Diagnostics;
using System.Threading.Channels;
using Moongate.Abstractions.Services.Base;
using Moongate.Server.Data.Packets;
using Moongate.Server.Interfaces.Services;
using Serilog;

namespace Moongate.Server.Services;

public class GameLoopService : BaseMoongateService, IGameLoopService, IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Channel<GamePacket> _inboundPackets;
    private readonly ILogger _logger = Log.ForContext<GameLoopService>();
    private readonly TimeSpan _tickInterval = TimeSpan.FromMilliseconds(250);

    public long TickCount { get; private set; }
    public TimeSpan Uptime { get; private set; }
    public double AverageTickMs { get; private set; }

    public GameLoopService()
        => _inboundPackets = Channel.CreateUnbounded<GamePacket>(
               new()
               {
                   SingleReader = true,
                   SingleWriter = false
               }
           );

    public void Dispose()
    {
        if (!_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }

        _cancellationTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }

    public void EnqueueGamePacket(GamePacket gamePacket)
    {
        if (!_inboundPackets.Writer.TryWrite(gamePacket))
        {
            _logger.Warning("Failed to enqueue game packet: {GamePacket}", gamePacket);
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

    private void ProcessQueue() { }
}
