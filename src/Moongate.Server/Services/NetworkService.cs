using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using Moongate.Network.Client;
using Moongate.Network.Events;
using Moongate.Network.Packets.Data.Packets;
using Moongate.Network.Packets.Registry;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Server;
using Moongate.Server.Data.Packets;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Listener;
using Moongate.Server.Interfaces.Services;
using Serilog;

namespace Moongate.Server.Services;

public class NetworkService : INetworkService
{
    private readonly ILogger _logger = Log.ForContext<NetworkService>();
    private readonly IGamePacketIngress _gamePacketIngress;
    private readonly IPacketDispatchService _packetDispatchService;
    private readonly IGameNetworkSessionService _gameNetworkSessionService;

    private readonly ConcurrentDictionary<long, MoongateTCPClient> _connectedClients = new();

    private readonly List<MoongateTCPServer> _tcpServers = new();

    private readonly PacketRegistry _packetRegistry = new();
    private readonly ConcurrentQueue<GamePacket> _parsedPackets = new();

    public IReadOnlyCollection<GamePacket> ParsedPackets => _parsedPackets.ToArray();

    public NetworkService(
        IGamePacketIngress gamePacketIngress,
        IPacketDispatchService packetDispatchService,
        IGameNetworkSessionService gameNetworkSessionService
    )
    {
        _gamePacketIngress = gamePacketIngress;
        _packetDispatchService = packetDispatchService;
        _gameNetworkSessionService = gameNetworkSessionService;
        PacketTable.Register(_packetRegistry);

        ShowRegisteredPackets();
    }

    public void AddPacketListener(byte OpCode, IPacketListener packetListener)
        => _packetDispatchService.AddPacketListener(OpCode, packetListener);

    private void ShowRegisteredPackets()
    {
        _logger.Information("Registered packets:");

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

    public async Task StartAsync()
    {
        foreach (var ipEndpoint in GetListeningAddresses(new IPEndPoint(IPAddress.Any, 2593)))
        {
            var moongateTcpServer = new MoongateTCPServer(new IPEndPoint(ipEndpoint.Address, 2593));

            moongateTcpServer.OnClientConnect += OnClientConnected;
            moongateTcpServer.OnClientDisconnect += OnClientDisconnected;
            moongateTcpServer.OnDataReceived += OnClientData;
            moongateTcpServer.OnException += OnClientException;

            _tcpServers.Add(moongateTcpServer);

            await moongateTcpServer.StartAsync(CancellationToken.None);
        }
    }

    private void OnClientException(object? sender, MoongateTCPExceptionEventArgs e)
    {
        _logger.Error(e.Exception, "Client exception: {Message}", e.Exception.Message);
    }

    private void OnClientData(object? sender, MoongateTCPDataReceivedEventArgs e)
    {
        if (e.Data.IsEmpty)
        {
            return;
        }

        var session = _gameNetworkSessionService.GetOrCreate(e.Client);
        session.WithPendingBytesLock(
            pendingBytes =>
            {
                pendingBytes.AddRange(e.Data.Span.ToArray());
                ParseAvailablePackets(pendingBytes, session);
            }
        );
    }

    private void OnClientDisconnected(object? sender, MoongateTCPClientEventArgs e)
    {
        _logger.Information("Client disconnected: {RemoteEndPoint}", e.Client.RemoteEndPoint);

        _connectedClients.TryRemove(e.Client.SessionId, out _);
        _gameNetworkSessionService.Remove(e.Client.SessionId);
    }

    private void OnClientConnected(object? sender, MoongateTCPClientEventArgs e)
    {
        _logger.Information("Client connected: {RemoteEndPoint}", e.Client.RemoteEndPoint);

        _connectedClients.TryAdd(e.Client.SessionId, e.Client);
        _gameNetworkSessionService.GetOrCreate(e.Client);
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
        _connectedClients.Clear();
        _gameNetworkSessionService.Clear();

        while (_parsedPackets.TryDequeue(out _)) { }
    }

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }

    public bool TryDequeueParsedPacket(out GamePacket gamePacket)
        => _parsedPackets.TryDequeue(out gamePacket);

    private void ParseAvailablePackets(
        List<byte> pendingBytes,
        GameNetworkSession session
    )
    {
        while (pendingBytes.Count > 0)
        {
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
                _logger.Warning(
                    "Invalid packet length {Length} for opcode 0x{OpCode:X2}. Dropping 1 byte.",
                    expectedLength,
                    opCode
                );
                pendingBytes.RemoveAt(0);

                continue;
            }

            if (pendingBytes.Count < expectedLength)
            {
                break;
            }

            var rawPacket = new byte[expectedLength.Value];
            pendingBytes.CopyTo(0, rawPacket, 0, expectedLength.Value);
            pendingBytes.RemoveRange(0, expectedLength.Value);

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

            var gamePacket = new GamePacket(
                session,
                opCode,
                packet,
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            );

            _parsedPackets.Enqueue(gamePacket);
            _gamePacketIngress.EnqueueGamePacket(gamePacket);
            _logger.Information("Received packet 0x{OpCode:X2} from session {SessionId}", opCode, session.SessionId);
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
}
