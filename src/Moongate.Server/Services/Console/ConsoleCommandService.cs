using System.Text;
using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Services.Console;
using Moongate.Server.Interfaces.Services.Events;
using Serilog;
using Serilog.Events;

namespace Moongate.Server.Services.Console;

/// <summary>
/// Captures terminal input and publishes commands on the game event bus.
/// </summary>
public sealed class ConsoleCommandService : IConsoleCommandService, IDisposable
{
    private readonly IConsoleUiService _consoleUiService;
    private readonly IGameEventBusService _gameEventBusService;
    private readonly ILogger _logger = Log.ForContext<ConsoleCommandService>();
    private readonly List<string> _commandHistory = [];

    private CancellationTokenSource _lifetimeCts = new();
    private Task _inputLoopTask = Task.CompletedTask;
    private int _commandHistoryIndex = -1;

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
        _consoleUiService.LockInput();
        _consoleUiService.WriteLogLine(
            $"Console input is locked. Press '{_consoleUiService.UnlockCharacter}' to unlock.",
            LogEventLevel.Warning
        );
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
        var lockWarningShown = false;
        _consoleUiService.UpdateInput(string.Empty);

        while (!cancellationToken.IsCancellationRequested)
        {
            if (!System.Console.KeyAvailable)
            {
                await Task.Delay(25, cancellationToken);

                continue;
            }

            var key = System.Console.ReadKey(true);

            if (_consoleUiService.IsInputLocked)
            {
                if (key.KeyChar == _consoleUiService.UnlockCharacter)
                {
                    _consoleUiService.UnlockInput();
                    lockWarningShown = false;
                    _consoleUiService.WriteLogLine("Console unlocked.", LogEventLevel.Information);
                }
                else if (!lockWarningShown)
                {
                    _consoleUiService.WriteLogLine(
                        $"Console input is locked. Press '{_consoleUiService.UnlockCharacter}' to unlock.",
                        LogEventLevel.Warning
                    );
                    lockWarningShown = true;
                }

                continue;
            }

            if (key.Key == ConsoleKey.Enter)
            {
                await SubmitCommandAsync(buffer.ToString(), cancellationToken);
                buffer.Clear();
                _commandHistoryIndex = -1;
                _consoleUiService.UpdateInput(string.Empty);
                lockWarningShown = false;

                continue;
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (buffer.Length > 0)
                {
                    buffer.Length--;
                    _consoleUiService.UpdateInput(buffer.ToString());
                }

                lockWarningShown = false;

                continue;
            }

            if (key.Key == ConsoleKey.Escape)
            {
                buffer.Clear();
                _commandHistoryIndex = -1;
                _consoleUiService.UpdateInput(string.Empty);
                lockWarningShown = false;

                continue;
            }

            if (key.Key == ConsoleKey.UpArrow)
            {
                if (_commandHistory.Count == 0)
                {
                    continue;
                }

                if (_commandHistoryIndex < _commandHistory.Count - 1)
                {
                    _commandHistoryIndex++;
                }

                buffer.Clear();
                buffer.Append(_commandHistory[_commandHistory.Count - 1 - _commandHistoryIndex]);
                _consoleUiService.UpdateInput(buffer.ToString());
                lockWarningShown = false;

                continue;
            }

            if (key.Key == ConsoleKey.DownArrow)
            {
                if (_commandHistory.Count == 0)
                {
                    continue;
                }

                if (_commandHistoryIndex > 0)
                {
                    _commandHistoryIndex--;
                    buffer.Clear();
                    buffer.Append(_commandHistory[_commandHistory.Count - 1 - _commandHistoryIndex]);
                }
                else
                {
                    _commandHistoryIndex = -1;
                    buffer.Clear();
                }

                _consoleUiService.UpdateInput(buffer.ToString());
                lockWarningShown = false;

                continue;
            }

            if (key.Key == ConsoleKey.PageUp)
            {
                _consoleUiService.ScrollPageUp();
                lockWarningShown = false;

                continue;
            }

            if (key.Key == ConsoleKey.PageDown)
            {
                _consoleUiService.ScrollPageDown();
                lockWarningShown = false;

                continue;
            }

            if (key.Key == ConsoleKey.Home)
            {
                _consoleUiService.ScrollToTop();
                lockWarningShown = false;

                continue;
            }

            if (key.Key == ConsoleKey.End)
            {
                _consoleUiService.ScrollToBottom();
                lockWarningShown = false;

                continue;
            }

            if (!char.IsControl(key.KeyChar))
            {
                buffer.Append(key.KeyChar);
                _consoleUiService.UpdateInput(buffer.ToString());
                lockWarningShown = false;
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

        _commandHistory.Add(command);

        _logger.Verbose("Console command entered: {Command}", command);

        await _gameEventBusService.PublishAsync(
            new CommandEnteredEvent(command, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),
            cancellationToken
        );
    }
}
