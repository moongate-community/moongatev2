using System.Collections.Concurrent;
using System.Net;
using Moongate.Network.Client;
using Moongate.Network.Events;
using Moongate.Network.Packets.Registry;
using Moongate.Network.Server;
using Moongate.Server.Interfaces.Listener;
using Moongate.Server.Interfaces.Services;
using Serilog;

namespace Moongate.Server.Services;

public class NetworkService : INetworkService
{
    private readonly ILogger _logger = Log.ForContext<NetworkService>();

    private readonly Dictionary<byte, List<IPacketListener>> _packetListeners = new();

    private readonly ConcurrentDictionary<long, MoongateTCPClient> _connectedClients = new();

    private readonly List<MoongateTCPServer> _tcpServers = new();

    private readonly PacketRegistry _packetRegistry = new();

    public void AddPacketListener(byte OpCode, IPacketListener packetListener)
    {
        if (!_packetListeners.TryGetValue(OpCode, out var value))
        {
            value = new();
            _packetListeners[OpCode] = value;
        }

        _logger.Information("Adding packet listener for {OpCode}", OpCode);

        value.Add(packetListener);
    }

    public NetworkService()
    {
        PacketTable.Register(_packetRegistry);

        ShowRegisteredPackets();
    }

    private void ShowRegisteredPackets()
    {
        _logger.Information("Registered packets:");

        foreach (var packet in _packetRegistry.RegisteredPackets)
        {
            _logger.Debug(
                " - OpCode: 0x{OpCode:X2}, Type: {PacketType}, Sizing: {PacketSizing}, Length: {Length}",
                packet.OpCode,
                packet.HandlerType.Name,
                packet.Sizing,
                packet.Length
            );
        }
    }

    public async Task StartAsync()
    {
        var moongateTcpServer = new MoongateTCPServer(new IPEndPoint(IPAddress.Any, 2593));

        moongateTcpServer.OnClientConnect += OnClientConnected;
        moongateTcpServer.OnClientDisconnect += OnClientDisconnected;
        moongateTcpServer.OnDataReceived += OnClientData;
        moongateTcpServer.OnException += OnClientException;

        _tcpServers.Add(moongateTcpServer);

        await moongateTcpServer.StartAsync(CancellationToken.None);
    }

    private void OnClientException(object? sender, MoongateTCPExceptionEventArgs e)
    {
        _logger.Error(e.Exception, "Client exception: {Message}", e.Exception.Message);
    }

    private void OnClientData(object? sender, MoongateTCPDataReceivedEventArgs e) { }

    private void OnClientDisconnected(object? sender, MoongateTCPClientEventArgs e)
    {
        _logger.Information("Client disconnected: {RemoteEndPoint}", e.Client.RemoteEndPoint);

        _connectedClients.TryRemove(e.Client.SessionId, out _);
    }

    private void OnClientConnected(object? sender, MoongateTCPClientEventArgs e)
    {
        _logger.Information("Client connected: {RemoteEndPoint}", e.Client.RemoteEndPoint);

        _connectedClients.TryAdd(e.Client.SessionId, e.Client);
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
    }

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }
}
