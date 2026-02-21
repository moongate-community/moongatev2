using System.Text;
using Moongate.Network.Packets.Outgoing.Speech;
using Moongate.Server.Data.Events;
using Moongate.Server.Data.Internal.Commands;
using Moongate.Server.Data.Session;
using Moongate.Server.Interfaces.Services.Console;
using Moongate.Server.Interfaces.Services.Events;
using Moongate.Server.Interfaces.Services.Lifecycle;
using Moongate.Server.Interfaces.Services.Packets;
using Moongate.Server.Types.Commands;
using Moongate.UO.Data.Utils;
using Serilog;
using Serilog.Events;

namespace Moongate.Server.Services.Console;

/// <summary>
/// Implements registration and execution of built-in server commands.
/// </summary>
public sealed class CommandSystemService : ICommandSystemService, IGameEventListener<CommandEnteredEvent>
{
    private readonly ILogger _logger = Log.ForContext<CommandSystemService>();
    private readonly Dictionary<string, CommandDefinition> _commands = new(StringComparer.OrdinalIgnoreCase);
    private readonly IConsoleUiService _consoleUiService;
    private readonly IGameEventBusService _gameEventBusService;
    private readonly IOutgoingPacketQueue _outgoingPacketQueue;
    private readonly IServerLifetimeService _serverLifetimeService;

    public CommandSystemService(
        IConsoleUiService consoleUiService,
        IGameEventBusService gameEventBusService,
        IOutgoingPacketQueue outgoingPacketQueue,
        IServerLifetimeService serverLifetimeService
    )
    {
        _consoleUiService = consoleUiService;
        _gameEventBusService = gameEventBusService;
        _outgoingPacketQueue = outgoingPacketQueue;
        _serverLifetimeService = serverLifetimeService;
        RegisterDefaultCommands();
    }

    public async Task ExecuteCommandAsync(
        string commandWithArgs,
        CommandSourceType source = CommandSourceType.Console,
        GameSession? session = null,
        CancellationToken cancellationToken = default
    )
    {
        _logger.Verbose("Received command input '{CommandInput}' from source {Source}", commandWithArgs, source);

        if (string.IsNullOrWhiteSpace(commandWithArgs))
        {
            _logger.Verbose("Ignoring empty command input from source {Source}", source);

            return;
        }

        var tokens = commandWithArgs
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (tokens.Length == 0)
        {
            _logger.Verbose("Ignoring command input with no tokens from source {Source}", source);

            return;
        }

        var command = tokens[0].ToLowerInvariant();
        _logger.Verbose(
            "Parsed command '{Command}' with {ArgumentCount} args from source {Source}",
            command,
            tokens.Length - 1,
            source
        );

        if (!_commands.TryGetValue(command, out var commandDefinition))
        {
            _logger.Verbose("Command '{Command}' is not registered", command);
            WriteCommandOutput(source, session, LogEventLevel.Warning, "Unknown command: {0}", commandWithArgs);

            return;
        }

        if (!commandDefinition.Source.HasFlag(source))
        {
            _logger.Verbose(
                "Command '{Command}' is not allowed for source {Source}. Allowed source flags: {AllowedSource}",
                command,
                source,
                commandDefinition.Source
            );
            WriteCommandOutput(
                source,
                session,
                LogEventLevel.Warning,
                "Command '{0}' is not available from source '{1}'.",
                command,
                source
            );

            return;
        }

        var context = new CommandSystemContext(
            commandWithArgs,
            tokens.Skip(1).ToArray(),
            (message, level) => WriteCommandOutput(source, session, level, message)
        );

        _logger.Verbose("Executing command handler for '{Command}'", command);

        try
        {
            await commandDefinition.Handler(context);
            _logger.Verbose("Command '{Command}' executed successfully", command);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Command '{Command}' execution failed", command);
            WriteCommandOutput(
                source,
                session,
                LogEventLevel.Error,
                "Command '{0}' failed. Check logs for details.",
                command
            );
        }
    }

    public async Task HandleAsync(CommandEnteredEvent gameEvent, CancellationToken cancellationToken = default)
    {
        await ExecuteCommandAsync(
            gameEvent.CommandText,
            gameEvent.Source,
            gameEvent.GameSession,
            cancellationToken: cancellationToken
        );
    }

    public void RegisterCommand(
        string commandName,
        Func<CommandSystemContext, Task> handler,
        string description = "",
        CommandSourceType source = CommandSourceType.Console
    )
    {
        if (string.IsNullOrWhiteSpace(commandName))
        {
            throw new ArgumentException("Command name is required.", nameof(commandName));
        }

        var aliases = commandName.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var commandDefinition = new CommandDefinition
        {
            Name = aliases[0].Trim().ToLowerInvariant(),
            Description = description,
            Handler = handler,
            Source = source
        };

        foreach (var alias in aliases)
        {
            var normalizedAlias = alias.Trim().ToLowerInvariant();

            if (!_commands.TryAdd(normalizedAlias, commandDefinition))
            {
                _logger.Warning("Command '{CommandName}' is already registered.", normalizedAlias);

                continue;
            }

            _logger.Debug("Registered command {CommandName}", normalizedAlias);
        }
    }

    public Task StartAsync()
    {
        _gameEventBusService.RegisterListener(this);
        _logger.Information("Command system started with {CommandCount} command aliases.", _commands.Count);

        return Task.CompletedTask;
    }

    public Task StopAsync()
        => Task.CompletedTask;

    private Task OnExitCommand(CommandSystemContext context)
    {
        context.Print("Shutdown requested by console command.");
        _serverLifetimeService.RequestShutdown();

        return Task.CompletedTask;
    }

    private Task OnHelpCommand(CommandSystemContext context)
    {
        var uniqueCommands = _commands
                             .Values
                             .Distinct()
                             .OrderBy(command => command.Name, StringComparer.OrdinalIgnoreCase)
                             .ToArray();

        var builder = new StringBuilder();
        builder.AppendLine("Available commands:");

        foreach (var command in uniqueCommands)
        {
            builder.Append("- ");
            builder.Append(command.Name);

            if (!string.IsNullOrWhiteSpace(command.Description))
            {
                builder.Append(": ");
                builder.Append(command.Description);
            }

            builder.AppendLine();
        }

        context.Print(builder.ToString().TrimEnd());

        return Task.CompletedTask;
    }

    private Task OnLockCommand(CommandSystemContext context)
    {
        _consoleUiService.LockInput();
        context.Print(
            "Console input is locked. Press '{0}' to unlock.",
            _consoleUiService.UnlockCharacter
        );

        return Task.CompletedTask;
    }

    private void RegisterDefaultCommands()
    {
        RegisterCommand(
            "help|?",
            OnHelpCommand,
            "Displays available commands.",
            CommandSourceType.Console | CommandSourceType.InGame
        );
        RegisterCommand(
            "lock|*",
            OnLockCommand,
            "Locks console input. Press '*' to unlock."
        );
        RegisterCommand(
            "exit|shutdown",
            OnExitCommand,
            "Requests server shutdown."
        );
    }

    private void WriteCommandOutput(
        CommandSourceType source,
        GameSession? session,
        LogEventLevel level,
        string message,
        params object[] args
    )
    {
        var formatted = args.Length == 0 ? message : string.Format(message, args);

        if (source == CommandSourceType.InGame && session is not null)
        {
            WriteInGameOutput(session, formatted, level);

            return;
        }

        _consoleUiService.WriteLogLine(formatted, level);
    }

    private void WriteInGameOutput(GameSession session, string formatted, LogEventLevel level)
    {
        var hue = level switch
        {
            LogEventLevel.Error or LogEventLevel.Fatal => SpeechHues.Red,
            LogEventLevel.Warning => SpeechHues.Yellow,
            _ => SpeechHues.System
        };

        var lines = formatted.Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var normalized = line.TrimEnd('\r');

            if (normalized.Length == 0)
            {
                continue;
            }

            _outgoingPacketQueue.Enqueue(session.SessionId, SpeechMessageFactory.CreateSystem(normalized, hue: hue));
        }
    }
}
