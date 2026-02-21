using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.MegaCliloc;

namespace Moongate.Network.Packets.Incoming.System;

public record struct MegaClilocProperty(uint ClilocId, string? Text);

/// <summary>
/// Mega Cliloc packet (0xD6) - Used for tooltips and object properties
/// </summary>
/// <remarks>
/// Server Version (Outbound):
/// - BYTE[1] 0xD6
/// - BYTE[2] Length
/// - BYTE[2] 0x0001 (subcommand)
/// - BYTE[4] Serial of item/creature
/// - BYTE[2] 0x0000
/// - BYTE[4] Serial (repeated)
/// - Loop of properties:
///   - BYTE[4] Cliloc ID
///   - BYTE[2] Text length
///   - BYTE[?] Unicode text
/// - BYTE[4] 0x00000000 (terminator)
/// 
/// Client Version (Inbound):
/// - BYTE[1] 0xD6
/// - BYTE[2] Length
/// - Loop of serials to request tooltip for:
///   - BYTE[4] Serial
/// </remarks>
[PacketHandler(0xD6, PacketSizing.Variable, Description = "Mega Cliloc")]
public class MegaClilocPacket : BaseGameNetworkPacket
{
    /// <summary>
    /// Subcommand type (0x0001 for server tooltip, 0x0000 for client request)
    /// </summary>
    public ushort Subcommand { get; private set; }

    /// <summary>
    /// Serial of the object/creature this tooltip is for
    /// </summary>
    public Serial Serial { get; private set; }

    /// <summary>
    /// List of cliloc properties for this object
    /// </summary>
    public List<MegaClilocProperty> Properties { get; private set; } = new();

    /// <summary>
    /// For client requests: list of serials being requested
    /// </summary>
    public List<Serial> RequestedSerials { get; private set; } = new();

    /// <summary>
    /// Indicates if this is a client request (true) or server response (false)
    /// </summary>
    public bool IsClientRequest => Subcommand == 0x0000;

    public MegaClilocPacket()
        : base(0xD6) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining < 2)
        {
            return false;
        }

        var declaredLength = reader.ReadUInt16();

        if (declaredLength < 3)
        {
            return false;
        }

        var expectedPayloadLength = declaredLength - 3;

        if (reader.Remaining < expectedPayloadLength)
        {
            return false;
        }

        var payload = reader.ReadBytes(expectedPayloadLength);

        using var payloadReader = new SpanReader(payload);

        RequestedSerials.Clear();
        Properties.Clear();
        Subcommand = 0x0000;

        // Server tooltip payload starts with subcommand 0x0001.
        if (payloadReader.Remaining >= 2 && payloadReader.Buffer[..2].SequenceEqual(new byte[] { 0x00, 0x01 }))
        {
            Subcommand = payloadReader.ReadUInt16();

            if (payloadReader.Remaining < 10)
            {
                return false;
            }

            var firstSerial = payloadReader.ReadUInt32();
            _ = payloadReader.ReadUInt16(); // Always 0x0000
            _ = payloadReader.ReadUInt32(); // repeated serial
            Serial = new Serial(firstSerial);

            while (payloadReader.Remaining >= 4)
            {
                var clilocId = payloadReader.ReadUInt32();

                if (clilocId == 0)
                {
                    break;
                }

                if (payloadReader.Remaining < 2)
                {
                    return false;
                }

                var textLength = payloadReader.ReadUInt16();
                string? text = null;

                if (textLength > 0)
                {
                    if (payloadReader.Remaining < textLength)
                    {
                        return false;
                    }

                    text = global::System.Text.Encoding.Unicode.GetString(payloadReader.ReadBytes(textLength));
                }

                Properties.Add(new MegaClilocProperty(clilocId, text));
            }

            return true;
        }

        // Client request: payload is serial list.
        while (payloadReader.Remaining >= 4)
        {
            var serial = payloadReader.ReadUInt32();

            if (serial != 0)
            {
                RequestedSerials.Add(new Serial(serial));
            }
        }

        return RequestedSerials.Count > 0;
    }
}
