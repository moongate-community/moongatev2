using Moongate.Network.Packets.Interfaces;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Services;
using Moongate.Server.Listeners.Base;

namespace Moongate.Server.Handlers;

public class LoginHandler : BasePacketListener
{
    public LoginHandler(IOutgoingPacketQueue outgoingPacketQueue) : base(outgoingPacketQueue)
    {

    }

    protected override async Task<bool> HandleCoreAsync(GameSession session, IGameNetworkPacket packet)
    {
        return true;

    }
}
