using Moongate.Abstractions.Interfaces.Services.Base;
using Moongate.Server.Data.Packets;

namespace Moongate.Server.Interfaces.Services;

public interface IGameLoopService : IMoongateService
{
    void EnqueueGamePacket(GamePacket gamePacket);
}
