using System.Net;
using System.Net.Sockets;
using System.Text;
using Moongate.Network.Events;
using Moongate.Network.Server;
using Moongate.Tests.Network.Support;

namespace Moongate.Tests.Network;

public class MoongateTcpServerTests
{
    [Test]
    public async Task StartAsync_WhenClientConnectsAndSendsData_ShouldRaiseAllEvents()
    {
        using var server = new MoongateTCPServer(new(IPAddress.Loopback, 0));
        server.AddMiddleware(new MoongateTcpServerUppercaseMiddleware());

        var connectedTcs =
            new TaskCompletionSource<MoongateTCPClientEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
        var dataTcs =
            new TaskCompletionSource<MoongateTCPDataReceivedEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);

        server.OnClientConnect += (_, args) => connectedTcs.TrySetResult(args);
        server.OnDataReceived += (_, args) => dataTcs.TrySetResult(args);

        await server.StartAsync(CancellationToken.None);

        using var tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(IPAddress.Loopback, server.Port);

        var connected = await WaitAsync(connectedTcs.Task);
        Assert.That(connected.Client.SessionId, Is.GreaterThan(0));

        await tcpClient.GetStream().WriteAsync("hello"u8.ToArray());

        var dataReceived = await WaitAsync(dataTcs.Task);
        Assert.That(Encoding.UTF8.GetString(dataReceived.Data.Span), Is.EqualTo("HELLO"));

        await server.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task StartAsync_WhenClientSendsData_ShouldExposePeekAndConsumeApi()
    {
        using var server = new MoongateTCPServer(new(IPAddress.Loopback, 0));

        var dataTcs =
            new TaskCompletionSource<MoongateTCPDataReceivedEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
        server.OnDataReceived += (_, args) => dataTcs.TrySetResult(args);

        await server.StartAsync(CancellationToken.None);

        using var tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(IPAddress.Loopback, server.Port);
        await tcpClient.GetStream().WriteAsync("abcdef"u8.ToArray());

        var dataReceived = await WaitAsync(dataTcs.Task);
        var networkClient = dataReceived.Client;

        Assert.That(networkClient.AvailableBytes, Is.EqualTo(6));
        Assert.That(Encoding.UTF8.GetString(networkClient.PeekData()), Is.EqualTo("abcdef"));

        var consumed = networkClient.ConsumeBytes(2);
        Assert.That(consumed, Is.EqualTo(2));
        Assert.That(networkClient.AvailableBytes, Is.EqualTo(4));
        Assert.That(Encoding.UTF8.GetString(networkClient.PeekData()), Is.EqualTo("cdef"));

        await server.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task StartAsync_WhenClientSendsData_ShouldStoreRecentBytesInCircularBuffer()
    {
        using var server = new MoongateTCPServer(new(IPAddress.Loopback, 0));

        var dataTcs =
            new TaskCompletionSource<MoongateTCPDataReceivedEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
        server.OnDataReceived += (_, args) => dataTcs.TrySetResult(args);

        await server.StartAsync(CancellationToken.None);

        using var tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(IPAddress.Loopback, server.Port);
        await tcpClient.GetStream().WriteAsync("buffer"u8.ToArray());

        var dataReceived = await WaitAsync(dataTcs.Task);
        var recentBytes = dataReceived.Client.GetRecentReceivedBytes();

        Assert.That(Encoding.UTF8.GetString(recentBytes), Is.EqualTo("buffer"));

        await server.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task StartAsync_WhenMiddlewareThrows_ShouldRaiseOnException()
    {
        using var server = new MoongateTCPServer(new(IPAddress.Loopback, 0));
        server.AddMiddleware(new MoongateTcpServerThrowingMiddleware());

        var exceptionTcs =
            new TaskCompletionSource<MoongateTCPExceptionEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
        server.OnException += (_, args) => exceptionTcs.TrySetResult(args);

        await server.StartAsync(CancellationToken.None);

        using var tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(IPAddress.Loopback, server.Port);
        await tcpClient.GetStream().WriteAsync("x"u8.ToArray());

        var exceptionArgs = await WaitAsync(exceptionTcs.Task);

        Assert.That(exceptionArgs.Exception, Is.TypeOf<InvalidOperationException>());

        await server.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task StartAsync_WhenServerClosesClient_ShouldRaiseDisconnectEvent()
    {
        using var server = new MoongateTCPServer(new(IPAddress.Loopback, 0));

        var connectedTcs =
            new TaskCompletionSource<MoongateTCPClientEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
        var disconnectedTcs =
            new TaskCompletionSource<MoongateTCPClientEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
        server.OnClientConnect += (_, args) => connectedTcs.TrySetResult(args);
        server.OnClientDisconnect += (_, args) => disconnectedTcs.TrySetResult(args);

        await server.StartAsync(CancellationToken.None);

        using var tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(IPAddress.Loopback, server.Port);

        var connected = await WaitAsync(connectedTcs.Task);
        await connected.Client.CloseAsync();

        _ = await WaitAsync(disconnectedTcs.Task);

        await server.StopAsync(CancellationToken.None);
    }

    private static async Task<T> WaitAsync<T>(Task<T> task)
    {
        using var timeoutCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        return await task.WaitAsync(timeoutCancellationTokenSource.Token);
    }
}
