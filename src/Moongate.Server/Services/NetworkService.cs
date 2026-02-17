using System.Collections.Concurrent;
using Moongate.Server.Interfaces.Listener;
using Moongate.Server.Interfaces.Services;
using Serilog;

namespace Moongate.Server.Services;

public class NetworkService : INetworkService
{
    private readonly ILogger _logger = Log.ForContext<NetworkService>();

    private readonly Dictionary<byte, List<IPacketListener>> _packetListeners = new();


    public void AddPacketListener(byte OpCode, IPacketListener packetListener)
    {
        if (!_packetListeners.TryGetValue(OpCode, out var value))
        {
            value = new();
            _packetListeners[OpCode] = value;
        }

        _logger.Information("Adding packet listener for {OpCode}", OpCode);

        value.Add(packetListener);
    }
}
