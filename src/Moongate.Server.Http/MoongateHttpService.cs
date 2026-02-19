using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Http.Data;
using Moongate.Server.Http.Interfaces;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Globalization;
using System.Text;

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
    private readonly Func<MoongateHttpMetricsSnapshot?>? _metricsSnapshotFactory;

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
        _metricsSnapshotFactory = options.MetricsSnapshotFactory;
    }

    public async Task StartAsync()
    {
        if (_app is not null)
        {
            return;
        }

        var builder = WebApplication.CreateSlimBuilder(Array.Empty<string>());
        var logPath = CreateLogPath(_directoriesConfig[DirectoryType.Logs]);
        var httpLogger = CreateHttpLogger(logPath, _minimumLogLevel);

        builder.WebHost.UseUrls($"http://0.0.0.0:{_port}");
        builder.Host.UseSerilog(httpLogger, true);

        RegisterServiceMappings(builder.Services, _serviceMappings);

        if (_isOpenApiEnabled)
        {
            builder.Services.AddOpenApi();
        }

        var app = builder.Build();
        app.UseSerilogRequestLogging();

        app.MapGet(
            "/",
            static async context =>
            {
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("Moongate HTTP Service is running.");
            }
        );

        app.MapGet(
            "/health",
            static async context =>
            {
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("ok");
            }
        );

        app.MapGet(
            "/metrics",
            async context =>
            {
                if (_metricsSnapshotFactory is null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("metrics endpoint is not configured");
                    return;
                }

                var snapshot = _metricsSnapshotFactory();

                if (snapshot is null)
                {
                    context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("metrics are currently unavailable");
                    return;
                }

                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = "text/plain; version=0.0.4";
                await context.Response.WriteAsync(BuildPrometheusPayload(snapshot));
            }
        );

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

    private static Logger CreateHttpLogger(string logPath, LogEventLevel minimumLogLevel)
        => new LoggerConfiguration()
           .MinimumLevel
           .Is(minimumLogLevel)
           .Enrich
           .FromLogContext()
           .WriteTo
           .File(logPath, rollingInterval: RollingInterval.Day)
           .CreateLogger();

    private static string CreateLogPath(string logDirectory)
    {
        Directory.CreateDirectory(logDirectory);

        return Path.Combine(logDirectory, "moongate_http-.log");
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

    private static string BuildPrometheusPayload(MoongateHttpMetricsSnapshot snapshot)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# generated by moongate");
        sb.Append("# collected_at_unix_ms ").AppendLine(
            snapshot.CollectedAt.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture)
        );

        foreach (var (metricKey, metric) in snapshot.Metrics.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            var normalizedName = NormalizeMetricName(metricKey);
            sb.Append("moongate_")
              .Append(normalizedName)
              .Append(' ')
              .Append(metric.Value.ToString(CultureInfo.InvariantCulture))
              .AppendLine();
        }

        return sb.ToString();
    }

    private static string NormalizeMetricName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        var buffer = new char[value.Length];

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            buffer[i] = char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '_';
        }

        return new string(buffer);
    }
}
