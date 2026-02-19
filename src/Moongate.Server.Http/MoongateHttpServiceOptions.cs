using Microsoft.AspNetCore.Builder;
using Moongate.Core.Data.Directories;
using Serilog.Events;

namespace Moongate.Server.Http;

/// <summary>
/// Configuration options for <see cref="MoongateHttpService" />.
/// </summary>
public sealed class MoongateHttpServiceOptions
{
    /// <summary>
    /// Optional service mappings exposed to the internal ASP.NET Core container.
    /// </summary>
    public IReadOnlyDictionary<Type, Type>? ServiceMappings { get; init; }

    /// <summary>
    /// Shared directories configuration used by the server.
    /// </summary>
    public DirectoriesConfig? DirectoriesConfig { get; init; }

    /// <summary>
    /// HTTP listening port.
    /// </summary>
    public int Port { get; init; } = 8088;

    /// <summary>
    /// Enables OpenAPI endpoints.
    /// </summary>
    public bool IsOpenApiEnabled { get; init; } = true;

    /// <summary>
    /// Minimum log level for HTTP logs.
    /// </summary>
    public LogEventLevel MinimumLogLevel { get; init; } = LogEventLevel.Information;

    /// <summary>
    /// Optional callback for extra endpoint/middleware registrations.
    /// </summary>
    public Action<WebApplication>? ConfigureApp { get; init; }
}
