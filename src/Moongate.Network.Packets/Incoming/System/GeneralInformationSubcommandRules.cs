using Moongate.UO.Data.Types;

namespace Moongate.Network.Packets.Incoming.System;

internal static class GeneralInformationSubcommandRules
{
    public static bool IsValid(GeneralInformationSubcommandType type, ReadOnlySpan<byte> payload)
        => type switch
        {
            GeneralInformationSubcommandType.InitializeFastWalkPrevention => payload.Length == 24,
            GeneralInformationSubcommandType.AddKeyToFastWalkStack => payload.Length == 4,
            GeneralInformationSubcommandType.CloseGenericGump => payload.Length == 8,
            GeneralInformationSubcommandType.ScreenSize => payload.Length == 8,
            GeneralInformationSubcommandType.PartySystem => payload.Length >= 1,
            GeneralInformationSubcommandType.SetCursorHueSetMap => payload.Length == 1,
            GeneralInformationSubcommandType.WrestlingStun => payload.IsEmpty,
            GeneralInformationSubcommandType.ClientLanguage => payload.Length is 3 or 4,
            GeneralInformationSubcommandType.ClosedStatusGump => payload.Length == 4,
            GeneralInformationSubcommandType.Action3DClient => payload.Length == 4,
            GeneralInformationSubcommandType.ClientType => true,
            GeneralInformationSubcommandType.MegaClilocRequest => payload.Length >= 4,
            GeneralInformationSubcommandType.RequestPopupMenu => payload.Length == 4,
            GeneralInformationSubcommandType.DisplayPopupContextMenu => payload.Length >= 7,
            GeneralInformationSubcommandType.PopupEntrySelection => payload.Length == 6,
            GeneralInformationSubcommandType.CloseUserInterfaceWindows => payload.Length == 5,
            GeneralInformationSubcommandType.CodexOfWisdom => payload.Length == 4,
            GeneralInformationSubcommandType.EnableMapDiff => payload.Length >= 1,
            GeneralInformationSubcommandType.ExtendedStats => payload.Length >= 1,
            GeneralInformationSubcommandType.StatLockChange => payload.Length == 2,
            GeneralInformationSubcommandType.NewSpellbook => payload.Length == 8,
            GeneralInformationSubcommandType.SpellSelected => payload.Length == 2,
            GeneralInformationSubcommandType.HouseRevisionState => payload.Length == 8,
            GeneralInformationSubcommandType.HouseRevisionRequest => payload.Length == 4,
            GeneralInformationSubcommandType.CustomHousing => payload.Length >= 5,
            GeneralInformationSubcommandType.AosAbilityIconConfirm => payload.IsEmpty,
            GeneralInformationSubcommandType.Damage => payload.Length == 6,
            GeneralInformationSubcommandType.Unknown24 => true,
            GeneralInformationSubcommandType.SeAbilityChange => payload.Length == 1,
            GeneralInformationSubcommandType.MountSpeed => payload.Length == 1,
            GeneralInformationSubcommandType.ChangeRace => payload.Length == 2,
            GeneralInformationSubcommandType.UseTargetedItem => payload.Length == 8,
            GeneralInformationSubcommandType.CastTargetedSpell => payload.Length == 6,
            GeneralInformationSubcommandType.UseTargetedSkill => payload.Length == 6,
            GeneralInformationSubcommandType.KrHouseMenuGump => true,
            GeneralInformationSubcommandType.ToggleGargoyleFlying => payload.Length is 0 or 4,
            GeneralInformationSubcommandType.Invalid => true,
            _ => true
        };
}
