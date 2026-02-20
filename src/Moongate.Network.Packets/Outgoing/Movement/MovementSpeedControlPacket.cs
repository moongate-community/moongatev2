using Moongate.Network.Packets.Base;
using Moongate.Network.Spans;
using Moongate.UO.Data.Types;

namespace Moongate.Network.Packets.Outgoing.Movement;

/// <summary>
/// Outbound movement speed control packet encoded as General Information (0xBF, subcommand 0x26).
/// This packet intentionally has no PacketHandler attribute to avoid registry opcode collision
/// with GeneralInformationPacket.
/// </summary>
public class MovementSpeedControlPacket : BaseGameNetworkPacket
{
    public MovementSpeedControlType SpeedControl { get; set; }

    public MovementSpeedControlPacket()
        : base(0xBF, 6) { }

    public MovementSpeedControlPacket(MovementSpeedControlType speedControl)
        : this()
        => SpeedControl = speedControl;

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((ushort)0x0006);
        writer.Write((ushort)0x0026);
        writer.Write((byte)SpeedControl);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 5)
        {
            return false;
        }

        var length = reader.ReadUInt16();

        if (length != 6)
        {
            return false;
        }

        var subCommand = reader.ReadUInt16();

        if (subCommand != 0x0026)
        {
            return false;
        }

        SpeedControl = (MovementSpeedControlType)reader.ReadByte();

        return true;
    }
}
