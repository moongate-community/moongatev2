using System.Buffers.Binary;
using Moongate.Network.Packets.Incoming.System;
using Moongate.Network.Spans;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.Network.Packets;

public class GeneralInformationPacketTests
{
    [TestCaseSource(nameof(AllKnownSubcommands))]
    public void WriteAndParse_ShouldRoundtrip_AllKnownSubcommands(
        GeneralInformationSubcommandType subcommandType,
        byte[] payload
    )
    {
        var packet = GeneralInformationPacket.Create(subcommandType, payload);
        var bytes = Write(packet);

        Assert.Multiple(
            () =>
            {
                Assert.That(bytes[0], Is.EqualTo(0xBF));
                Assert.That(BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(1, 2)), Is.EqualTo((ushort)bytes.Length));
                Assert.That(BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(3, 2)), Is.EqualTo((ushort)subcommandType));
            }
        );

        var parsed = new GeneralInformationPacket();
        var ok = parsed.TryParse(bytes);

        Assert.Multiple(
            () =>
            {
                Assert.That(ok, Is.True);
                Assert.That(parsed.SubcommandType, Is.EqualTo(subcommandType));
                Assert.That(parsed.SubcommandData.ToArray(), Is.EqualTo(payload));
            }
        );
    }

    [Test]
    public void TryParse_ShouldFail_WhenSubcommandPayloadIsInvalid()
    {
        var raw = new byte[] { 0xBF, 0x00, 0x05, 0x00, 0x08 };
        var packet = new GeneralInformationPacket();

        var ok = packet.TryParse(raw);

        Assert.That(ok, Is.False);
    }

    [Test]
    public void Write_ShouldThrow_WhenSubcommandPayloadIsInvalid()
    {
        var packet = GeneralInformationPacket.Create(
            GeneralInformationSubcommandType.SetCursorHueSetMap,
            ReadOnlyMemory<byte>.Empty
        );

        Assert.Throws<InvalidOperationException>(() => Write(packet));
    }

    private static IEnumerable<TestCaseData> AllKnownSubcommands()
    {
        yield return Case(GeneralInformationSubcommandType.InitializeFastWalkPrevention, new byte[24]);
        yield return Case(GeneralInformationSubcommandType.AddKeyToFastWalkStack, [0, 1, 2, 3]);
        yield return Case(GeneralInformationSubcommandType.CloseGenericGump, new byte[8]);
        yield return Case(GeneralInformationSubcommandType.ScreenSize, [0, 0, 0x04, 0x00, 0x03, 0x20, 0, 0]);
        yield return Case(GeneralInformationSubcommandType.PartySystem, [0x01]);
        yield return Case(GeneralInformationSubcommandType.SetCursorHueSetMap, [0x00]);
        yield return Case(GeneralInformationSubcommandType.WrestlingStun, []);
        yield return Case(GeneralInformationSubcommandType.ClientLanguage, [0x45, 0x4E, 0x55]);
        yield return Case(GeneralInformationSubcommandType.ClosedStatusGump, new byte[4]);
        yield return Case(GeneralInformationSubcommandType.Action3DClient, new byte[4]);
        yield return Case(GeneralInformationSubcommandType.ClientType, [0x0A, 0, 0, 0, 0]);
        yield return Case(GeneralInformationSubcommandType.MegaClilocRequest, new byte[8]);
        yield return Case(GeneralInformationSubcommandType.RequestPopupMenu, new byte[4]);
        yield return Case(GeneralInformationSubcommandType.DisplayPopupContextMenu, new byte[7]);
        yield return Case(GeneralInformationSubcommandType.PopupEntrySelection, new byte[6]);
        yield return Case(GeneralInformationSubcommandType.CloseUserInterfaceWindows, [0x01, 0, 0, 0, 0]);
        yield return Case(GeneralInformationSubcommandType.CodexOfWisdom, new byte[4]);
        yield return Case(GeneralInformationSubcommandType.EnableMapDiff, [0x03]);
        yield return Case(GeneralInformationSubcommandType.ExtendedStats, [0x02]);
        yield return Case(GeneralInformationSubcommandType.StatLockChange, [0x00, 0x01]);
        yield return Case(GeneralInformationSubcommandType.NewSpellbook, new byte[8]);
        yield return Case(GeneralInformationSubcommandType.SpellSelected, [0x00, 0x01]);
        yield return Case(GeneralInformationSubcommandType.HouseRevisionState, new byte[8]);
        yield return Case(GeneralInformationSubcommandType.HouseRevisionRequest, new byte[4]);
        yield return Case(GeneralInformationSubcommandType.CustomHousing, new byte[5]);
        yield return Case(GeneralInformationSubcommandType.AosAbilityIconConfirm, []);
        yield return Case(GeneralInformationSubcommandType.Damage, [0, 0, 0, 1, 0, 10]);
        yield return Case(GeneralInformationSubcommandType.Unknown24, []);
        yield return Case(GeneralInformationSubcommandType.SeAbilityChange, [0x01]);
        yield return Case(GeneralInformationSubcommandType.MountSpeed, [0x01]);
        yield return Case(GeneralInformationSubcommandType.ChangeRace, [0x00, 0x01]);
        yield return Case(GeneralInformationSubcommandType.UseTargetedItem, new byte[8]);
        yield return Case(GeneralInformationSubcommandType.CastTargetedSpell, [0, 1, 0, 0, 0, 1]);
        yield return Case(GeneralInformationSubcommandType.UseTargetedSkill, [0, 1, 0, 0, 0, 1]);
        yield return Case(GeneralInformationSubcommandType.KrHouseMenuGump, [0x00]);
        yield return Case(GeneralInformationSubcommandType.ToggleGargoyleFlying, [0x00, 0x00, 0x01, 0x00]);
    }

    private static TestCaseData Case(GeneralInformationSubcommandType type, byte[] payload)
        => new TestCaseData(type, payload).SetName($"0xBF_{(ushort)type:X2}_{type}");

    private static byte[] Write(GeneralInformationPacket packet)
    {
        var writer = new SpanWriter(128, true);
        packet.Write(ref writer);
        var bytes = writer.ToArray();
        writer.Dispose();

        return bytes;
    }
}
