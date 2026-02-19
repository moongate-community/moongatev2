using Moongate.Network.Packets.Attributes;
using Moongate.Network.Packets.Base;
using Moongate.Network.Packets.Types.Packets;
using Moongate.Network.Spans;
using Moongate.UO.Data.Expansions;
using Moongate.UO.Data.Types;

namespace Moongate.Network.Packets.Outgoing.Login;

[PacketHandler(0xB9, PacketSizing.Fixed, Length = 5, Description = "Enable locked client features")]
public class SupportFeaturesPacket : BaseGameNetworkPacket
{
    public FeatureFlags Flags { get; set; }

    public SupportFeaturesPacket()
        : this(GetDefaultFlags()) { }

    public SupportFeaturesPacket(FeatureFlags flags)
        : base(0xB9, 5)
        => Flags = flags;

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((uint)Flags);
    }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;

    private static FeatureFlags GetDefaultFlags()
    {
        var flags =
            ExpansionInfo.Table is { Length: > 0 }
                ? ExpansionInfo.CoreExpansion.SupportedFeatures
                : FeatureFlags.ExpansionEJ;

        flags |= FeatureFlags.LiveAccount;
        flags |= FeatureFlags.SeventhCharacterSlot;

        return flags;
    }
}
