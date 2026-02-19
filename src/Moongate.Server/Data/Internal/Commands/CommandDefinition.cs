using Moongate.Server.Types.Commands;

namespace Moongate.Server.Data.Internal.Commands;

/// <summary>
/// Represents one command registration entry.
/// </summary>
public sealed class CommandDefinition
{
    public required string Name { get; init; }

    public required string Description { get; init; }

    public required Func<CommandSystemContext, Task> Handler { get; init; }

    public CommandSourceType Source { get; init; }
}
