using System.Diagnostics;
using Moongate.Abstractions.Services.Base;
using Moongate.Server.Interfaces.Services;
using Serilog;

namespace Moongate.Server.Services;

public class GameLoopService : BaseMoongateService, IGameLoopService, IDisposable
{
    public long TickCount { get; private set; }
    public TimeSpan Uptime { get; private set; }
    public double AverageTickMs { get; private set; }

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly ILogger _logger = Log.ForContext<GameLoopService>();

    private readonly TimeSpan _tickInterval = TimeSpan.FromMilliseconds(250);

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
                    AverageTickMs = (AverageTickMs * 0.95) + (elapsed.TotalMilliseconds * 0.05);

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
