using Moongate.Core.Types;

namespace Moongate.Core.Utils;

/// <summary>
/// Provides utilities for detecting the current platform.
/// </summary>
public static class PlatformUtils
{
    /// <summary>
    /// Gets the current platform type.
    /// </summary>
    /// <returns>The detected platform type.</returns>
    public static PlatformType GetCurrentPlatform()
    {
        if (IsRunningOnWindows())
        {
            return PlatformType.Windows;
        }

        if (IsRunningOnMacOS())
        {
            return PlatformType.MacOS;
        }

        return IsRunningOnLinux() ? PlatformType.Linux : PlatformType.Unknown;
    }

    /// <summary>
    /// Checks if the application is running on Linux.
    /// </summary>
    /// <returns>True if running on Linux, otherwise false.</returns>
    public static bool IsRunningOnLinux()
        => OperatingSystem.IsLinux();

    /// <summary>
    /// Checks if the application is running on macOS.
    /// </summary>
    /// <returns>True if running on macOS, otherwise false.</returns>
    public static bool IsRunningOnMacOS()
        => OperatingSystem.IsMacOS();

    /// <summary>
    /// Checks if the application is running on Windows.
    /// </summary>
    /// <returns>True if running on Windows, otherwise false.</returns>
    public static bool IsRunningOnWindows()
        => OperatingSystem.IsWindows();
}
