using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Types;
using UOMap = Moongate.UO.Data.Maps.Map;

namespace Moongate.Network.Packets.Incoming.System;

[PacketHandler(0xBF, PacketSizing.Variable, Description = "General Information Packet")]
public class GeneralInformationPacket : BaseGameNetworkPacket
{
    public GeneralInformationSubcommandType SubcommandType { get; set; }

    public ReadOnlyMemory<byte> SubcommandData { get; set; } = ReadOnlyMemory<byte>.Empty;

    public GeneralInformationPacket()
        : base(0xBF) { }

    public GeneralInformationPacket(GeneralInformationSubcommandType subcommandType, ReadOnlyMemory<byte> subcommandData)
        : this()
    {
        SubcommandType = subcommandType;
        SubcommandData = subcommandData;
    }

    public static GeneralInformationPacket Create(
        GeneralInformationSubcommandType subcommandType,
        ReadOnlyMemory<byte> subcommandData
    ) => new(subcommandType, subcommandData);

    public static GeneralInformationPacket CreateSetCursorHueSetMap(byte mapId)
        => new(GeneralInformationSubcommandType.SetCursorHueSetMap, new[] { mapId });

    public static GeneralInformationPacket CreateSetCursorHueSetMap(UOMap? map)
        => CreateSetCursorHueSetMap((byte)(map?.MapID ?? 0));

    public override void Write(ref SpanWriter writer)
    {
        if (!GeneralInformationSubcommandRules.IsValid(SubcommandType, SubcommandData.Span))
        {
            throw new InvalidOperationException(
                $"Invalid 0xBF payload for subcommand 0x{(ushort)SubcommandType:X2} (length {SubcommandData.Length})."
            );
        }

        writer.Write(OpCode);
        writer.Write((ushort)(5 + SubcommandData.Length));
        writer.Write((ushort)SubcommandType);

        if (!SubcommandData.IsEmpty)
        {
            writer.Write(SubcommandData.Span);
        }
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining < 4)
        {
            return false;
        }

        var length = reader.ReadUInt16();

        if (length < 5)
        {
            return false;
        }

        SubcommandType = (GeneralInformationSubcommandType)reader.ReadUInt16();
        var dataLength = length - 5;

        if (dataLength > reader.Remaining)
        {
            return false;
        }

        SubcommandData = dataLength == 0 ? ReadOnlyMemory<byte>.Empty : reader.ReadBytes(dataLength);

        if (!GeneralInformationSubcommandRules.IsValid(SubcommandType, SubcommandData.Span))
        {
            return false;
        }

        return true;
    }
}
