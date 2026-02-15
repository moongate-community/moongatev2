namespace Moongate.Core.Types;

/// <summary>
/// public enum LogLevelType : byte.
/// </summary>
public enum LogLevelType : byte
{
    /// <summary>
    /// No logging.
    /// </summary>
    None = 0,

    /// <summary>
    /// Trace level logging.
    /// </summary>
    Trace = 1,

    /// <summary>
    /// Debug level logging.
    /// </summary>
    Debug = 2,

    /// <summary>
    /// Information level logging.
    /// </summary>
    Information = 3,

    /// <summary>
    /// Warning level logging.
    /// </summary>
    Warning = 4,

    /// <summary>
    /// Error level logging.
    /// </summary>
    Error = 5,

    /// <summary>
    /// Critical level logging.
    /// </summary>
    Critical = 6
}
