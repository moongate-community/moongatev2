using Moongate.Abstractions.Interfaces.Services.Base;
using Moongate.Server.Data.Internal.Commands;
using Moongate.Server.Data.Session;
using Moongate.Server.Types.Commands;

namespace Moongate.Server.Interfaces.Services.Console;

/// <summary>
/// Registers and dispatches operator commands.
/// </summary>
public interface ICommandSystemService : IMoongateService
{
    /// <summary>
    /// Executes a raw command text.
    /// </summary>
    /// <param name="commandWithArgs">Raw command text including arguments.</param>
    /// <param name="source">Command source.</param>
    /// <param name="session"> If comes from in game, is not null</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ExecuteCommandAsync(
        string commandWithArgs,
        CommandSourceType source = CommandSourceType.Console,
        GameSession? session = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Registers one command or multiple aliases separated by <c>|</c>.
    /// </summary>
    /// <param name="commandName">Primary command name or aliases list.</param>
    /// <param name="handler">Command handler delegate.</param>
    /// <param name="description">Command help description.</param>
    /// <param name="source">Allowed command source.</param>
    void RegisterCommand(
        string commandName,
        Func<CommandSystemContext, Task> handler,
        string description = "",
        CommandSourceType source = CommandSourceType.Console
    );
}
