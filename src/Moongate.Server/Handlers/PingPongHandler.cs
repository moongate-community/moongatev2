using Moongate.Network.Packets.Incoming.Player;
using Moongate.Network.Packets.Interfaces;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Services.Packets;
using Moongate.Server.Listeners.Base;

namespace Moongate.Server.Handlers;

public class PingPongHandler : BasePacketListener
{
    public PingPongHandler(IOutgoingPacketQueue outgoingPacketQueue) : base(outgoingPacketQueue) { }

    protected override async Task<bool> HandleCoreAsync(GameSession session, IGameNetworkPacket packet)
    {
        if (packet is PingMessagePacket pingMessagePacket)
        {
            session.PingSequence = pingMessagePacket.Sequence;
            session.PingSequence = (byte)((session.PingSequence + 1) % 256);

            Enqueue(session, new PingMessagePacket(session.PingSequence));
        }

        return true;
    }
}
