namespace Moongate.UO.Data.Types;

/// <summary>
/// Movement speed control modes sent to clients (0xBF subcommand 0x26).
/// </summary>
public enum MovementSpeedControlType : byte
{
    Disable = 0,
    Mount = 1,
    Walk = 2
}
