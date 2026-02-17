namespace Moongate.Scripting.Data.Scripts;

/// <summary>
/// Represents the result of a script execution.
/// </summary>
public class ScriptResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the script execution was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the message associated with the script result.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the data returned by the script execution.
    /// </summary>
    public object? Data { get; set; }
}
