using Moongate.Server.Data.Internal.Commands;
using Moongate.Server.Interfaces.Services.Console;
using Moongate.Server.Types.Commands;

namespace Moongate.Tests.Server.Support;

public sealed class MockCommandSystemService : ICommandSystemService
{
    public Task ExecuteCommandAsync(string commandWithArgs, CommandSourceType source = CommandSourceType.Console, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void RegisterCommand(string commandName, Func<CommandSystemContext, Task> handler, string description = "", CommandSourceType source = CommandSourceType.Console)
    {
    }

    public Task StartAsync() => Task.CompletedTask;

    public Task StopAsync() => Task.CompletedTask;
}
