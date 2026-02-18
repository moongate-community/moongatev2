using Moongate.Network.Packets.Incoming.Login;
using Moongate.Network.Packets.Interfaces;
using Moongate.Network.Packets.Outgoing.Login;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Services;
using Moongate.Server.Listeners.Base;
using Moongate.UO.Data.Types;
using Serilog;

namespace Moongate.Server.Handlers;

public class LoginHandler : BasePacketListener
{
    private readonly ILogger _logger = Log.ForContext<LoginHandler>();

    public LoginHandler(IOutgoingPacketQueue outgoingPacketQueue) : base(outgoingPacketQueue) { }

    protected override async Task<bool> HandleCoreAsync(GameSession session, IGameNetworkPacket packet)
    {
        if (packet is LoginSeedPacket loginSeedPacket)
        {
            return await HandleLoginSeedPacketAsync(session, loginSeedPacket);
        }

        if (packet is AccountLoginPacket accountLoginPacket)
        {
            return await HandleAccountLoginPacketAsync(session, accountLoginPacket);
        }

        return true;
    }

    private Task<bool> HandleLoginSeedPacketAsync(GameSession session, LoginSeedPacket packet)
    {
        _logger.Information(
            "Received LoginSeedPacket from session {SessionId} with seed {Seed} and client version {ClientVersion}",
            session.SessionId,
            packet.Seed,
            packet.ClientVersion
        );

        return Task.FromResult(true);
    }

    private async Task<bool> HandleAccountLoginPacketAsync(GameSession session, AccountLoginPacket accountLoginPacket)
    {
        _logger.Information(
            "Received AccountLoginPacket from session {SessionId} with username {Username}",
            session.SessionId,
            accountLoginPacket.Account
        );

        Enqueue(session, new LoginDeniedPacket(UOLoginDeniedReason.IncorrectNameOrPassword));

        return true;
    }
}
