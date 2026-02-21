using Moongate.Network.Packets.Incoming.Speech;
using Moongate.Network.Packets.Interfaces;
using Moongate.Network.Packets.Outgoing.Speech;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Services.Console;
using Moongate.Server.Interfaces.Services.Packets;
using Moongate.Server.Listeners.Base;
using Moongate.Server.Types.Commands;

namespace Moongate.Server.Handlers;

public class SpeechHandler : BasePacketListener
{
    private readonly ICommandSystemService _commandSystemService;

    public SpeechHandler(IOutgoingPacketQueue outgoingPacketQueue, ICommandSystemService commandSystemService)
        : base(outgoingPacketQueue)
    {
        _commandSystemService = commandSystemService;
    }

    protected override async Task<bool> HandleCoreAsync(GameSession session, IGameNetworkPacket packet)
    {
        if (packet is not UnicodeSpeechPacket speechPacket)
        {
            return true;
        }

        var text = speechPacket.Text.Trim();

        if (text.Length == 0)
        {
            return true;
        }

        if (text.StartsWith('.'))
        {
            await _commandSystemService.ExecuteCommandAsync(text, CommandSourceType.InGame);

            return true;
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

        return true;
    }
}
