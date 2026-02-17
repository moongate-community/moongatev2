using Moongate.Abstractions.Interfaces.Services.Base;

namespace Moongate.Server.Interfaces.Services;

/// <summary>
/// Defines the main game-loop service contract and packet ingress endpoint.
/// </summary>
public interface IGameLoopService : IMoongateService, IGamePacketIngress;
