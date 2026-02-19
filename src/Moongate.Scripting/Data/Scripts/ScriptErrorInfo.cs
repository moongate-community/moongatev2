namespace Moongate.Scripting.Data.Scripts;

/// <summary>
/// Detailed information about a JavaScript execution error
/// </summary>
public class ScriptErrorInfo
{
    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the stack trace.
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Gets or sets the line number.
    /// </summary>
    public int? LineNumber { get; set; }

    /// <summary>
    /// Gets or sets the column number.
    /// </summary>
    public int? ColumnNumber { get; set; }

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Gets or sets the error type.
    /// </summary>
    public string? ErrorType { get; set; }

    /// <summary>
    /// Gets or sets the source code.
    /// </summary>
    public string? SourceCode { get; set; }

    /// <summary>
    /// Original TypeScript file name if source maps are available
    /// </summary>
    public string? OriginalFileName { get; set; }

    /// <summary>
    /// Original line number in TypeScript if source maps are available
    /// </summary>
    public int? OriginalLineNumber { get; set; }

    /// <summary>
    /// Original column number in TypeScript if source maps are available
    /// </summary>
    public int? OriginalColumnNumber { get; set; }
}
