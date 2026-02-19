using System.Net;
using Moongate.Network.Packets.Incoming.Login;
using Moongate.Network.Packets.Interfaces;
using Moongate.Network.Packets.Outgoing.Login;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Services.Accounting;
using Moongate.Server.Interfaces.Services.Packets;
using Moongate.Server.Listeners.Base;
using Moongate.UO.Data.Types;
using Serilog;

namespace Moongate.Server.Handlers;

public class LoginHandler : BasePacketListener
{
    private readonly ILogger _logger = Log.ForContext<LoginHandler>();


    private readonly IAccountService _accountService;
    private readonly ServerListPacket _serverListPacket;

    public LoginHandler(IOutgoingPacketQueue outgoingPacketQueue, IAccountService accountService) : base(outgoingPacketQueue)
    {
        _accountService = accountService;
        _serverListPacket = new();
        _serverListPacket.Shards.Add(
            new()
            {
                Index = 0,
                IpAddress = IPAddress.Parse("127.0.0.1"),
                ServerName = "Moongate"
            }
        );
    }

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

        if (packet is ServerSelectPacket serverSelectPacket)
        {
            return await HandleServerSelectPacketAsync(session, serverSelectPacket);
        }

        if (packet is GameLoginPacket gameLoginPacket)
        {
            return await HandleGameLoginPacketAsync(session, gameLoginPacket);
        }

        return true;
    }

    private async Task<bool> HandleAccountLoginPacketAsync(GameSession session, AccountLoginPacket accountLoginPacket)
    {
        _logger.Information(
            "Received AccountLoginPacket from session {SessionId} with username {Username}",
            session.SessionId,
            accountLoginPacket.Account
        );

        if (await _accountService.LoginAsync(accountLoginPacket.Account, accountLoginPacket.Password) == null)
        {

            Enqueue(session, new LoginDeniedPacket(UOLoginDeniedReason.IncorrectNameOrPassword));
            return true;
        }



        Enqueue(session, _serverListPacket);

        return true;
    }

    private async Task<bool> HandleGameLoginPacketAsync(GameSession session, GameLoginPacket gameLoginPacket)
    {
        _logger.Information(
            "Received GameLoginPacket from session {SessionId} with account name {AccountName}",
            session.SessionId,
            gameLoginPacket.AccountName
        );

        session.NetworkSession.EnableCompression();

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

    private async Task<bool> HandleServerSelectPacketAsync(GameSession session, ServerSelectPacket serverSelectPacket)
    {
        var selectedIndex = serverSelectPacket.SelectedServerIndex;
        var selectedShard = _serverListPacket.Shards[selectedIndex];

        var sessionKey = new Random().Next();

        var connectToServer = new ServerRedirectPacket
        {
            IPAddress = selectedShard.IpAddress,
            Port = 2593,
            SessionKey = (uint)sessionKey
        };

        session.NetworkSession.SetSeed((uint)sessionKey);

        Enqueue(session, connectToServer);

        return true;
    }
}
