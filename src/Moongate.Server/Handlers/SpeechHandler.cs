using Moongate.Network.Packets.Incoming.Speech;
using Moongate.Network.Packets.Interfaces;
using Moongate.Network.Packets.Outgoing.Speech;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Services.Packets;
using Moongate.Server.Listeners.Base;

namespace Moongate.Server.Handlers;

public class SpeechHandler : BasePacketListener
{
    public SpeechHandler(IOutgoingPacketQueue outgoingPacketQueue)
        : base(outgoingPacketQueue) { }

    protected override Task<bool> HandleCoreAsync(GameSession session, IGameNetworkPacket packet)
    {
        if (packet is not UnicodeSpeechPacket speechPacket)
        {
            return Task.FromResult(true);
        }

        var text = speechPacket.Text.Trim();

        if (text.Length == 0)
        {
            return Task.FromResult(true);
        }

        Enqueue(
            session,
            SpeechMessageFactory.CreateFromSpeaker(
                session.Character,
                speechPacket.MessageType,
                speechPacket.Hue,
                speechPacket.Font,
                speechPacket.Language,
                text
            )
        );

        return Task.FromResult(true);
    }
}
