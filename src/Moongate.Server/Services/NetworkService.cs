using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using Moongate.Network.Events;
using Moongate.Network.Packets.Data.Packets;
using Moongate.Network.Packets.Registry;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Server;
using Moongate.Server.Data.Config;
using Moongate.Server.Data.Events;
using Moongate.Server.Data.Packets;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Listener;
using Moongate.Server.Interfaces.Services;
using Serilog;

namespace Moongate.Server.Services;

public class NetworkService : INetworkService
{
    private readonly ILogger _logger = Log.ForContext<NetworkService>();
    private readonly ILogger _packetDataLogger = Log.ForContext<NetworkService>().ForContext("PacketData", true);
    private readonly IMessageBusService _messageBusService;
    private readonly IGameEventBusService _gameEventBusService;
    private readonly IPacketDispatchService _packetDispatchService;
    private readonly IGameNetworkSessionService _gameNetworkSessionService;
    private readonly bool _logPacketData;

    private readonly List<MoongateTCPServer> _tcpServers = new();

    private readonly PacketRegistry _packetRegistry = new();
    private readonly ConcurrentQueue<IncomingGamePacket> _parsedPackets = new();

    public IReadOnlyCollection<IncomingGamePacket> ParsedPackets => _parsedPackets.ToArray();

    public NetworkService(
        IMessageBusService messageBusService,
        IGameEventBusService gameEventBusService,
        IPacketDispatchService packetDispatchService,
        IGameNetworkSessionService gameNetworkSessionService,
        MoongateConfig moongateConfig
    )
    {
        _messageBusService = messageBusService;
        _gameEventBusService = gameEventBusService;
        _packetDispatchService = packetDispatchService;
        _gameNetworkSessionService = gameNetworkSessionService;
        _logPacketData = moongateConfig.LogPacketData;
        PacketTable.Register(_packetRegistry);

        ShowRegisteredPackets();
    }

    public void AddPacketListener(byte OpCode, IPacketListener packetListener)
        => _packetDispatchService.AddPacketListener(OpCode, packetListener);

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }

    public static IEnumerable<IPEndPoint> GetListeningAddresses(IPEndPoint ipep)
    {
        return NetworkInterface.GetAllNetworkInterfaces()
                               .SelectMany(
                                   adapter =>
                                       adapter.GetIPProperties()
                                              .UnicastAddresses
                                              .Where(
                                                  uip => ipep.AddressFamily ==
                                                         uip.Address.AddressFamily
                                              )
                                              .Select(uip => new IPEndPoint(uip.Address, ipep.Port))
                               );
    }

    public async Task StartAsync()
    {
        foreach (var ipEndpoint in GetListeningAddresses(new(IPAddress.Any, 2593)))
        {
            var moongateTcpServer = new MoongateTCPServer(new(ipEndpoint.Address, 2593));

            moongateTcpServer.OnClientConnect += OnClientConnected;
            moongateTcpServer.OnClientDisconnect += OnClientDisconnected;
            moongateTcpServer.OnDataReceived += OnClientData;
            moongateTcpServer.OnException += OnClientException;

            _tcpServers.Add(moongateTcpServer);

            await moongateTcpServer.StartAsync(CancellationToken.None);
        }
    }

    public async Task StopAsync()
    {
        for (var i = _tcpServers.Count - 1; i >= 0; i--)
        {
            var server = _tcpServers[i];
            await server.StopAsync(CancellationToken.None);
            await server.DisposeAsync();
        }

        _tcpServers.Clear();
        _gameNetworkSessionService.Clear();

        while (_parsedPackets.TryDequeue(out _)) { }
    }

    public bool TryDequeueParsedPacket(out IncomingGamePacket gamePacket)
        => _parsedPackets.TryDequeue(out gamePacket);

    private static string BuildHexDump(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
        {
            return "<empty>";
        }

        var sb = new StringBuilder((data.Length / 16 + 1) * 80);

        for (var i = 0; i < data.Length; i += 16)
        {
            var lineLength = Math.Min(16, data.Length - i);
            sb.Append(i.ToString("X4"));
            sb.Append("  ");

            for (var j = 0; j < 16; j++)
            {
                if (j < lineLength)
                {
                    sb.Append(data[i + j].ToString("X2"));
                }
                else
                {
                    sb.Append("  ");
                }

                if (j != 15)
                {
                    sb.Append(' ');
                }
            }

            sb.Append("  |");

            for (var j = 0; j < lineLength; j++)
            {
                var b = data[i + j];
                sb.Append(b is >= 32 and <= 126 ? (char)b : '.');
            }

            sb.Append('|');

            if (i + lineLength < data.Length)
            {
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private void OnClientConnected(object? sender, MoongateTCPClientEventArgs e)
    {
        _logger.Information("Client connected: {RemoteEndPoint}", e.Client.RemoteEndPoint);

        var session = _gameNetworkSessionService.GetOrCreate(e.Client);
        session.NetworkSession.SetState(NetworkSessionState.AwaitingSeed);
        _ = PublishEventSafeAsync(
            new PlayerConnectedEvent(
                e.Client.SessionId,
                e.Client.RemoteEndPoint?.ToString(),
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            )
        );
    }

    private void OnClientData(object? sender, MoongateTCPDataReceivedEventArgs e)
    {
        if (e.Data.IsEmpty)
        {
            return;
        }

        var session = _gameNetworkSessionService.GetOrCreate(e.Client);
        session.NetworkSession.WithPendingBytesLock(
            pendingBytes =>
            {
                pendingBytes.AddRange(e.Data.Span.ToArray());
                ParseAvailablePackets(pendingBytes, session);
            }
        );
    }

    private void OnClientDisconnected(object? sender, MoongateTCPClientEventArgs e)
    {
        _logger.Information("Client disconnected: {SessionId}", e.Client.SessionId);

        if (_gameNetworkSessionService.TryGet(e.Client.SessionId, out var session))
        {
            session.NetworkSession.SetState(NetworkSessionState.Disconnecting);
            session.NetworkSession.DetachClient();
        }

        _gameNetworkSessionService.Remove(e.Client.SessionId);
        _ = PublishEventSafeAsync(
            new PlayerDisconnectedEvent(
                e.Client.SessionId,
                e.Client.RemoteEndPoint?.ToString(),
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            )
        );
    }

    private void OnClientException(object? sender, MoongateTCPExceptionEventArgs e)
    {
        _logger.Error(e.Exception, "Client exception: {Message}", e.Exception.Message);
    }

    private void ParseAvailablePackets(
        List<byte> pendingBytes,
        GameSession session
    )
    {
        while (pendingBytes.Count > 0)
        {
            if (!TryProcessInitialHandshake(pendingBytes, session))
            {
                break;
            }

            if (pendingBytes.Count == 0)
            {
                break;
            }

            var opCode = pendingBytes[0];

            if (!_packetRegistry.TryGetDescriptor(opCode, out var descriptor))
            {
                _logger.Warning(
                    "Unknown packet opcode 0x{OpCode:X2} for session {SessionId}. Dropping 1 byte.",
                    opCode,
                    session.SessionId
                );
                pendingBytes.RemoveAt(0);

                continue;
            }

            var expectedLength = ResolvePacketLength(pendingBytes, descriptor);

            if (expectedLength is null)
            {
                break;
            }

            if (expectedLength <= 0)
            {
                var bytesToDrop = descriptor.Sizing == PacketSizing.Variable && pendingBytes.Count >= 3 ? 3 : 1;
                _logger.Warning(
                    "Invalid packet length {Length} for opcode 0x{OpCode:X2}. Dropping {BytesToDrop} byte(s).",
                    expectedLength,
                    opCode,
                    bytesToDrop
                );
                pendingBytes.RemoveRange(0, bytesToDrop);

                continue;
            }

            if (pendingBytes.Count < expectedLength)
            {
                break;
            }

            var rawPacket = new byte[expectedLength.Value];
            pendingBytes.CopyTo(0, rawPacket, 0, expectedLength.Value);
            pendingBytes.RemoveRange(0, expectedLength.Value);

            if (_logPacketData)
            {
                _packetDataLogger.Information(
                    "Inbound packet Session={SessionId} OpCode=0x{OpCode:X2} Name={PacketName} Length={Length}{NewLine}{Dump}",
                    session.SessionId,
                    opCode,
                    descriptor.Description,
                    rawPacket.Length,
                    Environment.NewLine,
                    BuildHexDump(rawPacket)
                );
            }

            if (!_packetRegistry.TryCreatePacket(opCode, out var packet) || packet is null)
            {
                continue;
            }

            if (!packet.TryParse(rawPacket))
            {
                _logger.Warning(
                    "Failed to parse packet 0x{OpCode:X2} for session {SessionId}.",
                    opCode,
                    session.SessionId
                );

                continue;
            }

            var gamePacket = new IncomingGamePacket(
                session,
                opCode,
                packet,
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            );

            _parsedPackets.Enqueue(gamePacket);
            _messageBusService.PublishIncomingPacket(gamePacket);
            _logger.Information("Received packet 0x{OpCode:X2} from session {SessionId}", opCode, session.SessionId);
        }
    }

    private bool TryProcessInitialHandshake(List<byte> pendingBytes, GameSession session)
    {
        var networkSession = session.NetworkSession;

        if (networkSession.State != NetworkSessionState.AwaitingSeed)
        {
            return true;
        }

        var firstByte = pendingBytes[0];

        // 0xEF means this connection starts with the login-seed packet format.
        if (firstByte == PacketDefinition.LoginSeedPacket)
        {
            networkSession.SetState(NetworkSessionState.Login);
            return true;
        }

        // Game-server reconnect path: client can send a raw 4-byte seed first.
        if (pendingBytes.Count < 4)
        {
            return false;
        }

        Span<byte> seedBytes = stackalloc byte[4];
        seedBytes[0] = pendingBytes[0];
        seedBytes[1] = pendingBytes[1];
        seedBytes[2] = pendingBytes[2];
        seedBytes[3] = pendingBytes[3];
        var seed = BinaryPrimitives.ReadUInt32BigEndian(seedBytes);
        if (seed == 0)
        {
            _logger.Warning("Received invalid zero seed from session {SessionId}.", session.SessionId);
            pendingBytes.Clear();
            return false;
        }

        networkSession.SetSeed(seed);
        networkSession.SetState(NetworkSessionState.Login);
        pendingBytes.RemoveRange(0, 4);

        _logger.Information("Session {SessionId} completed seed handshake with 0x{Seed:X8}.", session.SessionId, seed);

        return true;
    }

    private async Task PublishEventSafeAsync<TEvent>(TEvent gameEvent) where TEvent : IGameEvent
    {
        try
        {
            await _gameEventBusService.PublishAsync(gameEvent);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to publish game event {EventType}", typeof(TEvent).Name);
        }
    }

    private static int? ResolvePacketLength(List<byte> pendingBytes, PacketDescriptor descriptor)
    {
        if (descriptor.Sizing == PacketSizing.Fixed)
        {
            return descriptor.Length;
        }

        if (pendingBytes.Count < 3)
        {
            return null;
        }

        Span<byte> lengthBuffer = stackalloc byte[2];
        lengthBuffer[0] = pendingBytes[1];
        lengthBuffer[1] = pendingBytes[2];

        return BinaryPrimitives.ReadUInt16BigEndian(lengthBuffer);
    }

    private void ShowRegisteredPackets()
    {
        _logger.Information("Registered packets: {Count}", _packetRegistry.RegisteredPackets.Count);

        foreach (var packet in _packetRegistry.RegisteredPackets)
        {
            _logger.Verbose(
                " - OpCode: 0x{OpCode:X2}, Type: {PacketType}, Sizing: {PacketSizing}, Length: {Length}, Description: {Description}",
                packet.OpCode,
                packet.HandlerType.Name,
                packet.Sizing,
                packet.Length,
                packet.Description
            );
        }
    }
}
