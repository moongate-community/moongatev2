using System.Text;
using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Services;
using Serilog;

namespace Moongate.Server.Services;

/// <summary>
/// Captures terminal input and publishes commands on the game event bus.
/// </summary>
public sealed class ConsoleCommandService : IConsoleCommandService, IDisposable
{
    private readonly IConsoleUiService _consoleUiService;
    private readonly IGameEventBusService _gameEventBusService;
    private readonly ILogger _logger = Log.ForContext<ConsoleCommandService>();

    private CancellationTokenSource _lifetimeCts = new();
    private Task _inputLoopTask = Task.CompletedTask;

    public ConsoleCommandService(IConsoleUiService consoleUiService, IGameEventBusService gameEventBusService)
    {
        _consoleUiService = consoleUiService;
        _gameEventBusService = gameEventBusService;
    }

    public void Dispose()
    {
        _lifetimeCts.Dispose();
        GC.SuppressFinalize(this);
    }

    public Task StartAsync()
    {
        if (!_consoleUiService.IsInteractive)
        {
            _logger.Information("Interactive console prompt disabled (non-interactive terminal).");

            return Task.CompletedTask;
        }

        if (_lifetimeCts.IsCancellationRequested)
        {
            _lifetimeCts.Dispose();
            _lifetimeCts = new();
        }

        _logger.Information("Interactive console prompt enabled.");
        _inputLoopTask = Task.Run(() => InputLoopAsync(_lifetimeCts.Token), _lifetimeCts.Token);

        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        _lifetimeCts.Cancel();

        try
        {
            await _inputLoopTask;
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }
    }

    private async Task InputLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = new StringBuilder();
        _consoleUiService.UpdateInput(string.Empty);

        while (!cancellationToken.IsCancellationRequested)
        {
            if (!Console.KeyAvailable)
            {
                await Task.Delay(25, cancellationToken);

                continue;
            }

            var key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Enter)
            {
                await SubmitCommandAsync(buffer.ToString(), cancellationToken);
                buffer.Clear();
                _consoleUiService.UpdateInput(string.Empty);

                continue;
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (buffer.Length > 0)
                {
                    buffer.Length--;
                    _consoleUiService.UpdateInput(buffer.ToString());
                }

                continue;
            }

            if (key.Key == ConsoleKey.Escape)
            {
                buffer.Clear();
                _consoleUiService.UpdateInput(string.Empty);

                continue;
            }

            if (!char.IsControl(key.KeyChar))
            {
                buffer.Append(key.KeyChar);
                _consoleUiService.UpdateInput(buffer.ToString());
            }
        }
    }

    private async Task SubmitCommandAsync(string rawCommand, CancellationToken cancellationToken)
    {
        var command = rawCommand.Trim();

        if (command.Length == 0)
        {
            return;
        }

        _logger.Verbose("Console command entered: {Command}", command);

        await _gameEventBusService.PublishAsync(
            new CommandEnteredEvent(command, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),
            cancellationToken
        );
    }
}
