using Moongate.Network.Client;
using Moongate.Server.Data.Packets;
using Moongate.Server.Interfaces.Services.Packets;

namespace Moongate.Tests.Server.Support;

public sealed class GameLoopTestOutboundPacketSender : IOutboundPacketSender
{
    public List<OutgoingGamePacket> SentPackets { get; } = [];

    public bool Send(MoongateTCPClient client, OutgoingGamePacket outgoingPacket)
    {
        SentPackets.Add(outgoingPacket);
        return true;
    }

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
