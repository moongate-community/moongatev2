namespace Moongate.Server.Data.Events;

/// <summary>
/// Event emitted when a console command is submitted by an operator.
/// </summary>
/// <param name="CommandText">Command text entered in the server console.</param>
/// <param name="Timestamp">Unix timestamp in milliseconds when the command was submitted.</param>
public readonly record struct CommandEnteredEvent(
    string CommandText,
    long Timestamp
) : IGameEvent;
