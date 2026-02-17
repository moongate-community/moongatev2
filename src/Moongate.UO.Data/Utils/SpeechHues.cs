namespace Moongate.UO.Data.Utils;

public static class SpeechHues
{
    public const short Default = 0x3B2;

    /// Standard speech hue (blue-gray) - most common
    public const short White = 0x1153;

    /// Bright white for emphasis
    public const short Orange = 0x59;

    /// Orange/yellow for warnings and system messages
    public const short BrightBlue = 0x35;

    /// Bright blue for important system messages
    public const short Green = 0x3F;

    /// Green for success messages
    public const short Red = 0x26;

    /// Red for error/danger messages
    public const short Yellow = 0x36;

    /// Yellow for notifications
    public const short System = 0x482;

    /// System message hue
    /// <summary>
    /// Default font for all speech (standard UO font)
    /// </summary>
    public const int DefaultFont = 3;

    /// <summary>
    /// Default graphic for speech (standard UO speech graphic)
    /// </summary>
    public const int DefaultGraphic = 0;
}
