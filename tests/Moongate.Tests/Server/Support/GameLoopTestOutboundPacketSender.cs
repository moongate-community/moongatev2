using Moongate.Network.Client;
using Moongate.Server.Data.Packets;
using Moongate.Server.Interfaces.Services;
using Moongate.Server.Interfaces.Services.Packets;

namespace Moongate.Tests.Server.Support;

public sealed class GameLoopTestOutboundPacketSender : IOutboundPacketSender
{
    public List<OutgoingGamePacket> SentPackets { get; } = [];

    public Task<bool> SendAsync(
        MoongateTCPClient client,
        OutgoingGamePacket outgoingPacket,
        CancellationToken cancellationToken
    )
    {
        SentPackets.Add(outgoingPacket);

        return Task.FromResult(true);
    }
}
