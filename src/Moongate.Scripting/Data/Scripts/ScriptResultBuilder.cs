namespace Moongate.Scripting.Data.Scripts;

/// <summary>
/// Builder class for creating ScriptResult instances.
/// </summary>
public class ScriptResultBuilder
{
    private object? _data;
    private string? _message;
    private bool _success;

    /// <summary>
    /// Builds the ScriptResult instance.
    /// </summary>
    public ScriptResult Build()
        => new()
        {
            Success = _success,
            Message = _message ?? "",
            Data = _data
        };

    /// <summary>
    /// Creates a ScriptResultBuilder initialized for an error result.
    /// </summary>
    public static ScriptResultBuilder CreateError()
        => new ScriptResultBuilder().WithSuccess(false);

    /// <summary>
    /// Creates a ScriptResultBuilder initialized for a successful result.
    /// </summary>
    public static ScriptResultBuilder CreateSuccess()
        => new ScriptResultBuilder().WithSuccess(true);

    /// <summary>
    /// Sets the result as failed.
    /// </summary>
    public ScriptResultBuilder Failure()
    {
        _success = false;

        return this;
    }

    /// <summary>
    /// Sets the result as successful.
    /// </summary>
    public ScriptResultBuilder Success()
    {
        _success = true;

        return this;
    }

    /// <summary>
    /// Sets the data of the result.
    /// </summary>
    public ScriptResultBuilder WithData(object? data)
    {
        _data = data;

        return this;
    }

    /// <summary>
    /// Sets the message of the result.
    /// </summary>
    public ScriptResultBuilder WithMessage(string message)
    {
        _message = message;

        return this;
    }

    /// <summary>
    /// Sets the success status of the result.
    /// </summary>
    public ScriptResultBuilder WithSuccess(bool success)
    {
        _success = success;

        return this;
    }
}
