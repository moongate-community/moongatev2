using Moongate.Network.Packets.Base;
using Moongate.Network.Spans;
using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.Network.Packets.Outgoing.World;

/// <summary>
/// Outbound war mode state packet (opcode 0x72).
/// This packet intentionally has no PacketHandler attribute to avoid registry opcode collision
/// with inbound RequestWarModePacket, since registry is currently direction-agnostic.
/// </summary>
public class WarModePacket : BaseGameNetworkPacket
{
    public UOMobileEntity? Mobile { get; set; }

    public WarModePacket()
        : base(0x72, 5) { }

    public WarModePacket(UOMobileEntity mobile)
        : this()
        => Mobile = mobile;

    public override void Write(ref SpanWriter writer)
    {
        if (Mobile is null)
        {
            throw new InvalidOperationException("Mobile must be set before writing WarModePacket.");
        }

        writer.Write(OpCode);
        writer.Write(Mobile.IsWarMode);
        writer.Write((byte)0);
        writer.Write((byte)0x32);
        writer.Write((byte)0);
    }

    protected override bool ParsePayload(ref SpanReader reader)
        => reader.Remaining == 4;
}
