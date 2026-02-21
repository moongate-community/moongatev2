using System.Collections.Concurrent;
using Moongate.Network.Packets.Registry;
using Moongate.Server.Data.Packets;
using Moongate.Server.Interfaces.Listener;
using Moongate.Server.Interfaces.Services.Packets;
using Serilog;

namespace Moongate.Server.Services.Packets;

public class PacketDispatchService : IPacketDispatchService
{
    private static readonly PacketRegistry _packetRegistry = CreatePacketRegistry();
    private readonly ILogger _logger = Log.ForContext<PacketDispatchService>();
    private readonly ConcurrentDictionary<byte, List<IPacketListener>> _packetListeners = new();

    public void AddPacketListener(byte opCode, IPacketListener packetListener)
    {
        var listeners = _packetListeners.GetOrAdd(opCode, static _ => []);

        lock (listeners)
        {
            listeners.Add(packetListener);
        }

        _logger.Information("Added packet listener for opcode 0x{OpCode:X2}", opCode);
    }

    public bool NotifyPacketListeners(IncomingGamePacket gamePacket)
    {
        var opCode = gamePacket.PacketId;

        if (!_packetListeners.TryGetValue(opCode, out var listeners))
        {
            if (_packetRegistry.TryGetDescriptor(opCode, out var descriptor))
            {
                _logger.Warning(
                    "No packet listeners for opcode 0x{OpCode:X2} ({Description})",
                    opCode,
                    descriptor.Description
                );
            }
            else
            {
                _logger.Warning("No packet listeners for opcode 0x{OpCode:X2}", opCode);
            }

            return false;
        }

        IPacketListener[] snapshot;

        lock (listeners)
        {
            if (listeners.Count == 0)
            {
                return false;
            }

            snapshot = listeners.ToArray();
        }

        var tasks = snapshot.Select(l => NotifyListenerSafeAsync(opCode, gamePacket, l));
        Task.WhenAll(tasks).GetAwaiter().GetResult();

        return true;
    }

    private async Task NotifyListenerSafeAsync(byte opCode, IncomingGamePacket gamePacket, IPacketListener listener)
    {
        try
        {
            _ = await listener.HandlePacketAsync(gamePacket.Session, gamePacket.Packet);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Listener failed for packet opcode 0x{OpCode:X2}", opCode);
        }
    }

    private static PacketRegistry CreatePacketRegistry()
    {
        var registry = new PacketRegistry();
        PacketTable.Register(registry);

        return registry;
    }
}
