namespace Moongate.UO.Data.Types;

[Flags]
public enum CharacterListFlags
{
    None = 0x00000000,
    Unk1 = 0x00000001,
    OverwriteConfigButton = 0x00000002,
    OneCharacterSlot = 0x00000004,
    ContextMenus = 0x00000008,
    SlotLimit = 0x00000010,
    AOS = 0x00000020,
    SixthCharacterSlot = 0x00000040,
    SE = 0x00000080,
    ML = 0x00000100,
    KR = 0x00000200,
    UO3DClientType = 0x00000400,
    Unk3 = 0x00000800,
    SeventhCharacterSlot = 0x00001000,
    Unk4 = 0x00002000,
    NewMovementSystem = 0x00004000, // Doesn't seem to be used on OSI
    NewFeluccaAreas = 0x00008000,

    ExpansionNone = ContextMenus,
    ExpansionT2A = ContextMenus,
    ExpansionUOR = ContextMenus,
    ExpansionUOTD = ContextMenus,
    ExpansionLBR = ContextMenus,
    ExpansionAOS = ContextMenus | AOS,
    ExpansionSE = ExpansionAOS | SE,
    ExpansionML = ExpansionSE | ML,
    ExpansionSA = ExpansionML,
    ExpansionHS = ExpansionSA,
    ExpansionTOL = ExpansionHS,
    ExpansionEJ = ExpansionTOL
}
