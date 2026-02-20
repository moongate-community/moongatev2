using System.Net.Sockets;
using Moongate.Network.Client;
using Moongate.Server.Services.Sessions;

namespace Moongate.Tests.Server;

public class GameNetworkSessionServiceTests
{
    [Test]
    public void GetAll_ShouldReturnSnapshotOfActiveSessions()
    {
        var service = new GameNetworkSessionService();

        using var clientA = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
        using var clientB = new MoongateTCPClient(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));

        var sessionA = service.GetOrCreate(clientA);
        var sessionB = service.GetOrCreate(clientB);

        var snapshot = service.GetAll();

        Assert.Multiple(
            () =>
            {
                Assert.That(snapshot.Count, Is.EqualTo(2));
                Assert.That(snapshot, Does.Contain(sessionA));
                Assert.That(snapshot, Does.Contain(sessionB));
            }
        );
    }
}
