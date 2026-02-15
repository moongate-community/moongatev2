using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Moongate.Network.Client;
using Moongate.Network.Events;
using Moongate.Network.Interfaces;
using Serilog;

namespace Moongate.Network.Server;

/// <summary>
/// High-throughput TCP server with client lifecycle events and middleware-enabled payload dispatch.
/// </summary>
public sealed class MoongateTCPServer : IAsyncDisposable, IDisposable
{
    private readonly ILogger _logger = Log.ForContext<MoongateTCPServer>();
    private readonly Lock _middlewareSync = new();
    private INetMiddleware[] _middlewares = [];
    private readonly ConcurrentDictionary<long, MoongateTCPClient> _clients = new();
    private readonly Socket _serverSocket;
    private readonly int _receiveBufferSize;
    private readonly int _historyBufferCapacity;

    private CancellationTokenSource? _listenerCancellationTokenSource;
    private Task? _acceptLoopTask;
    private int _started;

    /// <summary>
    /// Initializes a TCP server bound to the given endpoint.
    /// </summary>
    /// <param name="endPoint">Endpoint to bind to. Use port 0 for dynamic port assignment.</param>
    /// <param name="receiveBufferSize">Per-client receive buffer size.</param>
    /// <param name="historyBufferCapacity">Per-client circular history capacity.</param>
    public MoongateTCPServer(IPEndPoint endPoint, int receiveBufferSize = 8192, int historyBufferCapacity = 65536)
    {
        _serverSocket = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _serverSocket.Bind(endPoint);

        _receiveBufferSize = receiveBufferSize;
        _historyBufferCapacity = historyBufferCapacity;
    }

    /// <summary>
    /// Raised when a client connects.
    /// </summary>
    public event EventHandler<MoongateTCPClientEventArgs>? OnClientConnect;

    /// <summary>
    /// Raised when a client disconnects.
    /// </summary>
    public event EventHandler<MoongateTCPClientEventArgs>? OnClientDisconnect;

    /// <summary>
    /// Raised when a client sends data after middleware processing.
    /// </summary>
    public event EventHandler<MoongateTCPDataReceivedEventArgs>? OnDataReceived;

    /// <summary>
    /// Raised when an exception happens in accept loop or client loops.
    /// </summary>
    public event EventHandler<MoongateTCPExceptionEventArgs>? OnException;

    /// <summary>
    /// Current listening port.
    /// </summary>
    public int Port => ((IPEndPoint?)_serverSocket.LocalEndPoint)?.Port ?? 0;

    /// <summary>
    /// Registers middleware in execution order.
    /// </summary>
    public MoongateTCPServer AddMiddleware(INetMiddleware middleware)
    {
        lock (_middlewareSync)
        {
            _middlewares = [.. _middlewares, middleware];
        }

        return this;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        StopAsync(CancellationToken.None).GetAwaiter().GetResult();
        _listenerCancellationTokenSource?.Dispose();
        _serverSocket.Dispose();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await StopAsync(CancellationToken.None);

        _listenerCancellationTokenSource?.Dispose();
        _serverSocket.Dispose();
    }

    /// <summary>
    /// Starts accepting clients.
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _started, 1) != 0)
        {
            return Task.CompletedTask;
        }

        _serverSocket.Listen(512);
        _listenerCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _acceptLoopTask = Task.Run(AcceptLoopAsync, _listenerCancellationTokenSource.Token);

        _logger.Information("TCP server listening on {LocalEndPoint}", _serverSocket.LocalEndPoint);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops accepting new clients and closes all active clients.
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _started, 0) == 0)
        {
            return;
        }

        if (_listenerCancellationTokenSource is not null)
        {
            await _listenerCancellationTokenSource.CancelAsync();
        }

        try
        {
            _serverSocket.Close();
        }
        catch (SocketException)
        {
            // Listener may already be closed.
        }

        if (_acceptLoopTask is not null)
        {
            await _acceptLoopTask.WaitAsync(cancellationToken);
        }

        var clients = _clients.Values.ToArray();

        for (var i = 0; i < clients.Length; i++)
        {
            await clients[i].CloseAsync();
            await clients[i].DisposeAsync();
        }

        _clients.Clear();
    }

    private async Task AcceptLoopAsync()
    {
        if (_listenerCancellationTokenSource is null)
        {
            return;
        }

        while (!_listenerCancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                var clientSocket = await _serverSocket.AcceptAsync(_listenerCancellationTokenSource.Token);

                var middlewareSnapshot = _middlewares;
                var client = new MoongateTCPClient(
                    clientSocket,
                    middlewareSnapshot,
                    _receiveBufferSize,
                    _historyBufferCapacity
                );
                WireClientEvents(client);

                _clients[client.SessionId] = client;
                await client.StartAsync(_listenerCancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Accept loop failed");
                OnException?.Invoke(this, new(ex));
            }
        }
    }

    private void WireClientEvents(MoongateTCPClient client)
    {
        client.OnConnected += (_, args) =>
                              {
                                  _logger.Information(
                                      "OnClientConnect. SessionId={SessionId}, RemoteEndPoint={RemoteEndPoint}",
                                      args.Client.SessionId,
                                      args.Client.RemoteEndPoint
                                  );
                                  OnClientConnect?.Invoke(this, args);
                              };
        client.OnDataReceived += (_, args) =>
                                 {
                                     _logger.Debug(
                                         "OnDataReceived. SessionId={SessionId}, Bytes={Bytes}",
                                         args.Client.SessionId,
                                         args.Data.Length
                                     );
                                     OnDataReceived?.Invoke(this, args);
                                 };
        client.OnException += (_, args) =>
                              {
                                  _logger.Error(
                                      args.Exception,
                                      "OnException. SessionId={SessionId}",
                                      args.Client?.SessionId
                                  );
                                  OnException?.Invoke(this, args);
                              };
        client.OnDisconnected += (_, args) =>
                                 {
                                     _clients.TryRemove(args.Client.SessionId, out var _);
                                     _logger.Information(
                                         "OnClientDisconnect. SessionId={SessionId}, RemoteEndPoint={RemoteEndPoint}",
                                         args.Client.SessionId,
                                         args.Client.RemoteEndPoint
                                     );
                                     OnClientDisconnect?.Invoke(this, args);
                                 };
    }
}
