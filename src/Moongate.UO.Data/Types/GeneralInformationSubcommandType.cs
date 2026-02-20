namespace Moongate.UO.Data.Types;

/// <summary>
/// Subcommand types used by General Information packet (0xBF).
/// </summary>
public enum GeneralInformationSubcommandType : ushort
{
    Invalid = 0x00,
    SetCursorHueSetMap = 0x08
}
