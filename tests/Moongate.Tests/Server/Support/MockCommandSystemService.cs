using Moongate.Server.Data.Internal.Commands;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Services.Console;
using Moongate.Server.Types.Commands;

namespace Moongate.Tests.Server.Support;

public sealed class MockCommandSystemService : ICommandSystemService
{
    public int ExecuteCallCount { get; private set; }

    public string? LastCommandWithArgs { get; private set; }

    public CommandSourceType LastSource { get; private set; } = CommandSourceType.Console;

    public GameSession? LastSession { get; private set; }

    public CancellationToken LastCancellationToken { get; private set; }

    public Task ExecuteCommandAsync(
        string commandWithArgs,
        CommandSourceType source = CommandSourceType.Console,
        GameSession? session = null,
        CancellationToken cancellationToken = default
    )
    {
        ExecuteCallCount++;
        LastCommandWithArgs = commandWithArgs;
        LastSource = source;
        LastSession = session;
        LastCancellationToken = cancellationToken;

        return Task.CompletedTask;
    }

    public void RegisterCommand(string commandName, Func<CommandSystemContext, Task> handler, string description = "", CommandSourceType source = CommandSourceType.Console)
    {
    }

    public Task StartAsync() => Task.CompletedTask;

    public Task StopAsync() => Task.CompletedTask;
}
