namespace Moongate.Server.Types.Commands;

/// <summary>
/// Identifies where a command was submitted from.
/// </summary>
[Flags]
public enum CommandSourceType
{
    InGame = 1 << 0,
    Console = 1 << 1
}
