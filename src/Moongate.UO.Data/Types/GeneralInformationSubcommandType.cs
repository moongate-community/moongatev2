namespace Moongate.UO.Data.Types;

/// <summary>
/// Subcommand types used by General Information packet (0xBF).
/// </summary>
public enum GeneralInformationSubcommandType : ushort
{
    Invalid = 0x00,
    InitializeFastWalkPrevention = 0x01,
    AddKeyToFastWalkStack = 0x02,
    CloseGenericGump = 0x04,
    ScreenSize = 0x05,
    PartySystem = 0x06,
    SetCursorHueSetMap = 0x08,
    WrestlingStun = 0x0A,
    ClientLanguage = 0x0B,
    ClosedStatusGump = 0x0C,
    Action3DClient = 0x0E,
    ClientType = 0x0F,
    MegaClilocRequest = 0x10,
    RequestPopupMenu = 0x13,
    DisplayPopupContextMenu = 0x14,
    PopupEntrySelection = 0x15,
    CloseUserInterfaceWindows = 0x16,
    CodexOfWisdom = 0x17,
    EnableMapDiff = 0x18,
    ExtendedStats = 0x19,
    StatLockChange = 0x1A,
    NewSpellbook = 0x1B,
    SpellSelected = 0x1C,
    HouseRevisionState = 0x1D,
    HouseRevisionRequest = 0x1E,
    CustomHousing = 0x20,
    AosAbilityIconConfirm = 0x21,
    Damage = 0x22,
    Unknown24 = 0x24,
    SeAbilityChange = 0x25,
    MountSpeed = 0x26,
    ChangeRace = 0x2A,
    UseTargetedItem = 0x2C,
    CastTargetedSpell = 0x2D,
    UseTargetedSkill = 0x2E,
    KrHouseMenuGump = 0x2F,
    ToggleGargoyleFlying = 0x32
}
