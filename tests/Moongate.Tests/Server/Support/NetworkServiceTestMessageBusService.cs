using Moongate.Server.Data.Packets;
using Moongate.Server.Interfaces.Services;
using Moongate.Server.Interfaces.Services.Messaging;

namespace Moongate.Tests.Server.Support;

public sealed class NetworkServiceTestMessageBusService : IMessageBusService
{
    public List<IncomingGamePacket> Packets { get; } = [];

    public void PublishIncomingPacket(IncomingGamePacket packet)
        => Packets.Add(packet);

    public bool TryReadIncomingPacket(out IncomingGamePacket packet)
    {
        if (Packets.Count == 0)
        {
            packet = default;

            return false;
        }

        packet = Packets[0];
        Packets.RemoveAt(0);

        return true;
    }
}
