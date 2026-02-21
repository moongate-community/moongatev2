using Moongate.Network.Packets.Incoming.System;
using Moongate.UO.Data.Types;

namespace Moongate.Network.Packets.Outgoing.World;

/// <summary>
/// Builder for General Information packet (0xBF) subcommands.
/// </summary>
public static class GeneralInformationPacketBuilder
{
    public static GeneralInformationPacket Create(GeneralInformationSubcommandType subcommandType, ReadOnlySpan<byte> payload)
        => CreateChecked(subcommandType, payload);

    public static GeneralInformationPacket CreateInitializeFastWalkPrevention(
        uint key1,
        uint key2,
        uint key3,
        uint key4,
        uint key5,
        uint key6
    )
    {
        Span<byte> payload = stackalloc byte[24];
        WriteUInt32(payload, 0, key1);
        WriteUInt32(payload, 4, key2);
        WriteUInt32(payload, 8, key3);
        WriteUInt32(payload, 12, key4);
        WriteUInt32(payload, 16, key5);
        WriteUInt32(payload, 20, key6);

        return CreateChecked(GeneralInformationSubcommandType.InitializeFastWalkPrevention, payload);
    }

    public static GeneralInformationPacket CreateAddKeyToFastWalkStack(uint key)
    {
        Span<byte> payload = stackalloc byte[4];
        WriteUInt32(payload, 0, key);
        return CreateChecked(GeneralInformationSubcommandType.AddKeyToFastWalkStack, payload);
    }

    public static GeneralInformationPacket CreateCloseGenericGump(uint dialogId, uint buttonId)
    {
        Span<byte> payload = stackalloc byte[8];
        WriteUInt32(payload, 0, dialogId);
        WriteUInt32(payload, 4, buttonId);
        return CreateChecked(GeneralInformationSubcommandType.CloseGenericGump, payload);
    }

    public static GeneralInformationPacket CreateScreenSize(ushort unknown1, ushort x, ushort y, ushort unknown2)
    {
        Span<byte> payload = stackalloc byte[8];
        WriteUInt16(payload, 0, unknown1);
        WriteUInt16(payload, 2, x);
        WriteUInt16(payload, 4, y);
        WriteUInt16(payload, 6, unknown2);
        return CreateChecked(GeneralInformationSubcommandType.ScreenSize, payload);
    }

    public static GeneralInformationPacket CreatePartySystem(ReadOnlySpan<byte> payload)
        => CreateChecked(GeneralInformationSubcommandType.PartySystem, payload);

    public static GeneralInformationPacket CreateSetCursorHueSetMap(byte mapId)
        => CreateChecked(GeneralInformationSubcommandType.SetCursorHueSetMap, [mapId]);

    public static GeneralInformationPacket CreateWrestlingStun()
        => CreateChecked(GeneralInformationSubcommandType.WrestlingStun, []);

    public static GeneralInformationPacket CreateClientLanguage(ReadOnlySpan<byte> payload)
        => CreateChecked(GeneralInformationSubcommandType.ClientLanguage, payload);

    public static GeneralInformationPacket CreateClosedStatusGump(uint serial)
    {
        Span<byte> payload = stackalloc byte[4];
        WriteUInt32(payload, 0, serial);
        return CreateChecked(GeneralInformationSubcommandType.ClosedStatusGump, payload);
    }

    public static GeneralInformationPacket CreateAction3DClient(uint animationId)
    {
        Span<byte> payload = stackalloc byte[4];
        WriteUInt32(payload, 0, animationId);
        return CreateChecked(GeneralInformationSubcommandType.Action3DClient, payload);
    }

    public static GeneralInformationPacket CreateClientType(ReadOnlySpan<byte> payload)
        => CreateChecked(GeneralInformationSubcommandType.ClientType, payload);

    public static GeneralInformationPacket CreateMegaClilocRequest(ReadOnlySpan<byte> payload)
        => CreateChecked(GeneralInformationSubcommandType.MegaClilocRequest, payload);

    public static GeneralInformationPacket CreateRequestPopupMenu(uint serial)
    {
        Span<byte> payload = stackalloc byte[4];
        WriteUInt32(payload, 0, serial);
        return CreateChecked(GeneralInformationSubcommandType.RequestPopupMenu, payload);
    }

    public static GeneralInformationPacket CreateDisplayPopupContextMenu(ReadOnlySpan<byte> payload)
        => CreateChecked(GeneralInformationSubcommandType.DisplayPopupContextMenu, payload);

    public static GeneralInformationPacket CreatePopupEntrySelection(uint serial, ushort entryTag)
    {
        Span<byte> payload = stackalloc byte[6];
        WriteUInt32(payload, 0, serial);
        WriteUInt16(payload, 4, entryTag);
        return CreateChecked(GeneralInformationSubcommandType.PopupEntrySelection, payload);
    }

    public static GeneralInformationPacket CreateCloseUserInterfaceWindows(ReadOnlySpan<byte> payload)
        => CreateChecked(GeneralInformationSubcommandType.CloseUserInterfaceWindows, payload);

    public static GeneralInformationPacket CreateCodexOfWisdom(ReadOnlySpan<byte> payload)
        => CreateChecked(GeneralInformationSubcommandType.CodexOfWisdom, payload);

    public static GeneralInformationPacket CreateEnableMapDiff(ReadOnlySpan<byte> payload)
        => CreateChecked(GeneralInformationSubcommandType.EnableMapDiff, payload);

    public static GeneralInformationPacket CreateExtendedStats(ReadOnlySpan<byte> payload)
        => CreateChecked(GeneralInformationSubcommandType.ExtendedStats, payload);

    public static GeneralInformationPacket CreateStatLockChange(byte statIndex, byte lockValue)
        => CreateChecked(GeneralInformationSubcommandType.StatLockChange, [statIndex, lockValue]);

    public static GeneralInformationPacket CreateNewSpellbook(ReadOnlySpan<byte> payload)
        => CreateChecked(GeneralInformationSubcommandType.NewSpellbook, payload);

    public static GeneralInformationPacket CreateSpellSelected(ReadOnlySpan<byte> payload)
        => CreateChecked(GeneralInformationSubcommandType.SpellSelected, payload);

    public static GeneralInformationPacket CreateHouseRevisionState(ReadOnlySpan<byte> payload)
        => CreateChecked(GeneralInformationSubcommandType.HouseRevisionState, payload);

    public static GeneralInformationPacket CreateHouseRevisionRequest(ReadOnlySpan<byte> payload)
        => CreateChecked(GeneralInformationSubcommandType.HouseRevisionRequest, payload);

    public static GeneralInformationPacket CreateCustomHousing(ReadOnlySpan<byte> payload)
        => CreateChecked(GeneralInformationSubcommandType.CustomHousing, payload);

    public static GeneralInformationPacket CreateAosAbilityIconConfirm()
        => CreateChecked(GeneralInformationSubcommandType.AosAbilityIconConfirm, []);

    public static GeneralInformationPacket CreateDamage(uint serial, ushort amount)
    {
        Span<byte> payload = stackalloc byte[6];
        WriteUInt32(payload, 0, serial);
        WriteUInt16(payload, 4, amount);
        return CreateChecked(GeneralInformationSubcommandType.Damage, payload);
    }

    public static GeneralInformationPacket CreateUnknown24(ReadOnlySpan<byte> payload)
        => CreateChecked(GeneralInformationSubcommandType.Unknown24, payload);

    public static GeneralInformationPacket CreateSeAbilityChange(byte value)
        => CreateChecked(GeneralInformationSubcommandType.SeAbilityChange, [value]);

    public static GeneralInformationPacket CreateMountSpeed(byte speedControl)
        => CreateChecked(GeneralInformationSubcommandType.MountSpeed, [speedControl]);

    public static GeneralInformationPacket CreateChangeRace(ReadOnlySpan<byte> payload)
        => CreateChecked(GeneralInformationSubcommandType.ChangeRace, payload);

    public static GeneralInformationPacket CreateUseTargetedItem(uint itemSerial, uint targetSerial)
    {
        Span<byte> payload = stackalloc byte[8];
        WriteUInt32(payload, 0, itemSerial);
        WriteUInt32(payload, 4, targetSerial);
        return CreateChecked(GeneralInformationSubcommandType.UseTargetedItem, payload);
    }

    public static GeneralInformationPacket CreateCastTargetedSpell(short spellId, uint targetSerial)
    {
        Span<byte> payload = stackalloc byte[6];
        WriteInt16(payload, 0, spellId);
        WriteUInt32(payload, 2, targetSerial);
        return CreateChecked(GeneralInformationSubcommandType.CastTargetedSpell, payload);
    }

    public static GeneralInformationPacket CreateUseTargetedSkill(short skillId, uint targetSerial)
    {
        Span<byte> payload = stackalloc byte[6];
        WriteInt16(payload, 0, skillId);
        WriteUInt32(payload, 2, targetSerial);
        return CreateChecked(GeneralInformationSubcommandType.UseTargetedSkill, payload);
    }

    public static GeneralInformationPacket CreateKrHouseMenuGump(ReadOnlySpan<byte> payload)
        => CreateChecked(GeneralInformationSubcommandType.KrHouseMenuGump, payload);

    public static GeneralInformationPacket CreateToggleGargoyleFlying(ReadOnlySpan<byte> payload)
        => CreateChecked(GeneralInformationSubcommandType.ToggleGargoyleFlying, payload);

    private static GeneralInformationPacket CreateChecked(GeneralInformationSubcommandType subcommandType, ReadOnlySpan<byte> payload)
    {
        var data = payload.ToArray();

        if (!GeneralInformationSubcommandRules.IsValid(subcommandType, data))
        {
            throw new ArgumentException(
                $"Invalid payload for 0xBF subcommand 0x{(ushort)subcommandType:X2} (length {data.Length}).",
                nameof(payload)
            );
        }

        return GeneralInformationPacket.Create(subcommandType, data);
    }

    private static void WriteInt16(Span<byte> buffer, int offset, short value)
    {
        buffer[offset] = (byte)(value >> 8);
        buffer[offset + 1] = (byte)value;
    }

    private static void WriteUInt16(Span<byte> buffer, int offset, ushort value)
    {
        buffer[offset] = (byte)(value >> 8);
        buffer[offset + 1] = (byte)value;
    }

    private static void WriteUInt32(Span<byte> buffer, int offset, uint value)
    {
        buffer[offset] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)value;
    }
}
