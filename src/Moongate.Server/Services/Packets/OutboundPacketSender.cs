using System.Text;
using Moongate.Network.Client;
using Moongate.Network.Packets.Interfaces;
using Moongate.Network.Spans;
using Moongate.Server.Data.Config;
using Moongate.Server.Data.Packets;
using Moongate.Server.Interfaces.Services.Packets;
using Serilog;

namespace Moongate.Server.Services.Packets;

/// <summary>
/// Default outbound packet sender with payload serialization and packet data logging.
/// </summary>
public sealed class OutboundPacketSender : IOutboundPacketSender
{
    private readonly bool _logPacketData;
    private readonly ILogger _logger = Log.ForContext<OutboundPacketSender>();
    private readonly ILogger _packetDataLogger = Log.ForContext<OutboundPacketSender>().ForContext("PacketData", true);

    public OutboundPacketSender(MoongateConfig moongateConfig)
        => _logPacketData = moongateConfig.LogPacketData;

    public async Task<bool> SendAsync(
        MoongateTCPClient client,
        OutgoingGamePacket outgoingPacket,
        CancellationToken cancellationToken
    )
    {
        var payload = SerializePacket(outgoingPacket.Packet);

        if (payload.Length == 0)
        {
            return false;
        }

        if (_logPacketData)
        {
            _packetDataLogger.Information(
                "Outbound packet Session={SessionId} OpCode=0x{OpCode:X2} Name={PacketName} Length={Length}{NewLine}{Dump}",
                outgoingPacket.SessionId,
                outgoingPacket.Packet.OpCode,
                outgoingPacket.Packet.GetType().Name,
                payload.Length,
                Environment.NewLine,
                BuildHexDump(payload.AsSpan())
            );
        }

        try
        {
            await client.SendAsync(payload, cancellationToken);

            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(
                ex,
                "Failed sending outbound packet 0x{OpCode:X2} to session {SessionId}.",
                outgoingPacket.Packet.OpCode,
                outgoingPacket.SessionId
            );

            return false;
        }
    }

    private static string BuildHexDump(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
        {
            return "<empty>";
        }

        var sb = new StringBuilder((data.Length / 16 + 1) * 80);

        for (var i = 0; i < data.Length; i += 16)
        {
            var lineLength = Math.Min(16, data.Length - i);
            sb.Append(i.ToString("X4"));
            sb.Append("  ");

            for (var j = 0; j < 16; j++)
            {
                if (j < lineLength)
                {
                    sb.Append(data[i + j].ToString("X2"));
                }
                else
                {
                    sb.Append("  ");
                }

                if (j != 15)
                {
                    sb.Append(' ');
                }
            }

            sb.Append("  |");

            for (var j = 0; j < lineLength; j++)
            {
                var b = data[i + j];
                sb.Append(b is >= 32 and <= 126 ? (char)b : '.');
            }

            sb.Append('|');

            if (i + lineLength < data.Length)
            {
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static byte[] SerializePacket(IGameNetworkPacket packet)
    {
        var initialCapacity = packet.Length > 0 ? packet.Length : 256;
        var writer = new SpanWriter(initialCapacity, true);

        try
        {
            packet.Write(ref writer);

            return writer.ToArray();
        }
        finally
        {
            writer.Dispose();
        }
    }
}
