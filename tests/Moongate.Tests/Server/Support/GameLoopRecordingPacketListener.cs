using Moongate.Network.Packets.Interfaces;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Listener;

namespace Moongate.Tests.Server.Support;

public sealed class GameLoopRecordingPacketListener : IPacketListener
{
    public List<int> Sequences { get; } = [];

    public Task<bool> HandlePacketAsync(GameSession session, IGameNetworkPacket packet)
    {
        if (packet is GameLoopTestPacket testPacket)
        {
            Sequences.Add(testPacket.Sequence);
        }

        return Task.FromResult(true);
    }
}
