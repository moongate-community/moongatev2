using System.Diagnostics;
using System.Threading.Channels;
using Moongate.Abstractions.Services.Base;
using Moongate.Network.Spans;
using Moongate.Server.Data.Packets;
using Moongate.Server.Interfaces.Services;
using Serilog;

namespace Moongate.Server.Services;

public class GameLoopService : BaseMoongateService, IGameLoopService, IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Channel<IncomingGamePacket> _inboundPackets;
    private readonly IOutgoingPacketQueue _outgoingPacketQueue;
    private readonly IGameNetworkSessionService _gameNetworkSessionService;
    private readonly ILogger _logger = Log.ForContext<GameLoopService>();
    private readonly IPacketDispatchService _packetDispatchService;
    private readonly TimeSpan _tickInterval = TimeSpan.FromMilliseconds(250);

    public long TickCount { get; private set; }
    public TimeSpan Uptime { get; private set; }
    public double AverageTickMs { get; private set; }

    public GameLoopService(
        IPacketDispatchService packetDispatchService,
        IOutgoingPacketQueue outgoingPacketQueue,
        IGameNetworkSessionService gameNetworkSessionService
    )
    {
        _packetDispatchService = packetDispatchService;
        _outgoingPacketQueue = outgoingPacketQueue;
        _gameNetworkSessionService = gameNetworkSessionService;
        _inboundPackets = Channel.CreateUnbounded<IncomingGamePacket>(
            new()
            {
                SingleReader = true,
                SingleWriter = false
            }
        );

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

    public void EnqueueGamePacket(IncomingGamePacket gamePacket)
    {
        if (!_inboundPackets.Writer.TryWrite(gamePacket))
        {
            _logger.Warning("Failed to enqueue game packet: {IncomingGamePacket}", gamePacket);
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

    private void ProcessQueue()
    {
        DrainPacketQueue();
        DrainOutgoingPacketQueue();
    }

    private void DrainPacketQueue()
    {
        while (_inboundPackets.Reader.TryRead(out var gamePacket))
        {
            _packetDispatchService.NotifyPacketListeners(gamePacket);
        }
    }

    private void DrainOutgoingPacketQueue()
    {
        while (_outgoingPacketQueue.TryDequeue(out var outgoingPacket))
        {
            if (
                !_gameNetworkSessionService.TryGet(outgoingPacket.SessionId, out var session) ||
                session.Client is not { } client
            )
            {
                _logger.Warning(
                    "Skipping outbound packet 0x{OpCode:X2}: session {SessionId} is not connected.",
                    outgoingPacket.Packet.OpCode,
                    outgoingPacket.SessionId
                );

                continue;
            }

            var payload = SerializePacket(outgoingPacket.Packet);

            if (payload.Length == 0)
            {
                continue;
            }

            _ = SendPacketSafeAsync(client, outgoingPacket, payload);
        }
    }

    private static byte[] SerializePacket(Moongate.Network.Packets.Interfaces.IGameNetworkPacket packet)
    {
        var initialCapacity = packet.Length > 0 ? packet.Length : 256;
        var writer = new SpanWriter(initialCapacity, resize: true);

        try
        {
            packet.Write(ref writer);

            return writer.ToArray();
        }
        finally
        {
            writer.Dispose();
        }
    }

    private async Task SendPacketSafeAsync(
        Moongate.Network.Client.MoongateTCPClient client,
        OutgoingGamePacket outgoingPacket,
        ReadOnlyMemory<byte> payload
    )
    {
        try
        {
            await client.SendAsync(payload, _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown.
        }
        catch (Exception ex)
        {
            _logger.Error(
                ex,
                "Failed sending outbound packet 0x{OpCode:X2} to session {SessionId}.",
                outgoingPacket.Packet.OpCode,
                outgoingPacket.SessionId
            );
        }
    }
}
