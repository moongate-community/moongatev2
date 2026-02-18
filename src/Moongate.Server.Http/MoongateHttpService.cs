using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Http.Interfaces;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Moongate.Server.Http;

/// <summary>
/// Hosts a lightweight HTTP endpoint surface for diagnostics and admin APIs.
/// </summary>
public sealed class MoongateHttpService : IMoongateHttpService
{
    private readonly IReadOnlyDictionary<Type, Type> _serviceMappings;
    private readonly DirectoriesConfig _directoriesConfig;
    private readonly int _port;
    private readonly bool _isOpenApiEnabled;
    private readonly LogEventLevel _minimumLogLevel;
    private readonly Action<WebApplication> _configureApp;

    private WebApplication? _app;

    public MoongateHttpService(MoongateHttpServiceOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _serviceMappings = options.ServiceMappings ?? new Dictionary<Type, Type>();
        _directoriesConfig = options.DirectoriesConfig ??
                             throw new ArgumentException("DirectoriesConfig must be provided.", nameof(options));

        if (options.Port is <= 0 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Port must be in range 1-65535.");
        }

        _port = options.Port;
        _isOpenApiEnabled = options.IsOpenApiEnabled;
        _minimumLogLevel = options.MinimumLogLevel;
        _configureApp = options.ConfigureApp ?? (_ => { });
    }

    public async Task StartAsync()
    {
        if (_app is not null)
        {
            return;
        }

        var builder = WebApplication.CreateBuilder();
        var logPath = CreateLogPath(_directoriesConfig[DirectoryType.Logs]);
        var httpLogger = CreateHttpLogger(logPath, _minimumLogLevel);

        builder.WebHost.UseUrls($"http://0.0.0.0:{_port}");
        builder.Host.UseSerilog(httpLogger, dispose: true);

        RegisterServiceMappings(builder.Services, _serviceMappings);

        if (_isOpenApiEnabled)
        {
            builder.Services.AddOpenApi();
        }

        var app = builder.Build();
        app.UseSerilogRequestLogging();

        app.MapGet("/", () => "Moongate HTTP Service is running.");

        app.MapGet("/health", () => Results.Ok("ok"));

        if (_isOpenApiEnabled)
        {
            app.MapOpenApi();
            app.MapScalarApiReference(
                options =>
                {
                    options.Theme = ScalarTheme.BluePlanet;
                }
            );
        }

        _configureApp(app);

        await app.StartAsync();
        Log.Information("Moongate HTTP service started on port {Port}", _port);

        if (_isOpenApiEnabled)
        {
            Log.Information("OpenAPI documentation available at /scalar");
        }

        _app = app;
    }

    public async Task StopAsync()
    {
        if (_app is null)
        {
            return;
        }

        await _app.StopAsync();
        await _app.DisposeAsync();
        _app = null;
    }

    private static void RegisterServiceMappings(
        IServiceCollection services,
        IReadOnlyDictionary<Type, Type> mappings
    )
    {
        foreach (var (serviceType, implementationType) in mappings)
        {
            if (!serviceType.IsInterface)
            {
                throw new InvalidOperationException($"Service type '{serviceType.FullName}' must be an interface.");
            }

            if (implementationType.IsAbstract || implementationType.IsInterface)
            {
                throw new InvalidOperationException(
                    $"Implementation type '{implementationType.FullName}' must be a concrete class."
                );
            }

            if (!serviceType.IsAssignableFrom(implementationType))
            {
                throw new InvalidOperationException(
                    $"Implementation type '{implementationType.FullName}' does not implement '{serviceType.FullName}'."
                );
            }

            services.AddSingleton(serviceType, implementationType);
        }
    }

    private static string CreateLogPath(string logDirectory)
    {
        Directory.CreateDirectory(logDirectory);

        return Path.Combine(logDirectory, "moongate_http-.log");
    }

    private static Logger CreateHttpLogger(string logPath, LogEventLevel minimumLogLevel)
    {
        return new LoggerConfiguration()
               .MinimumLevel
               .Is(minimumLogLevel)
               .Enrich
               .FromLogContext()
               .WriteTo
               .File(logPath, rollingInterval: RollingInterval.Day)
               .CreateLogger();
    }
}
