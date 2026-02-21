using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Metrics.Data;
using Moongate.Server.Http.Data;
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
    private readonly Func<MoongateHttpMetricsSnapshot?>? _metricsSnapshotFactory;
    private readonly MoongateHttpJwtOptions _jwtOptions;
    private readonly Func<string, string, CancellationToken, Task<MoongateHttpAuthenticatedUser?>>? _authenticateUserAsync;

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
        _jwtOptions = options.Jwt ?? new MoongateHttpJwtOptions();
        _authenticateUserAsync = options.AuthenticateUserAsync;

        if (_jwtOptions.IsEnabled && string.IsNullOrWhiteSpace(_jwtOptions.SigningKey))
        {
            throw new ArgumentException("JWT signing key must be configured when JWT is enabled.", nameof(options));
        }

        if (_jwtOptions.IsEnabled && _authenticateUserAsync is null)
        {
            throw new ArgumentException("AuthenticateUserAsync must be configured when JWT is enabled.", nameof(options));
        }
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

        if (_jwtOptions.IsEnabled)
        {
            ConfigureJwt(builder.Services, _jwtOptions);
        }

        if (_isOpenApiEnabled)
        {
            builder.Services.AddOpenApi();
        }

        var app = builder.Build();
        app.UseSerilogRequestLogging();

        if (_jwtOptions.IsEnabled)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

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

        if (_jwtOptions.IsEnabled)
        {
            app.MapPost(
                "/auth/login",
                async (MoongateHttpLoginRequest request, CancellationToken cancellationToken) =>
                {
                    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                    {
                        return Results.BadRequest("username and password are required");
                    }

                    var user = await _authenticateUserAsync!(request.Username, request.Password, cancellationToken);

                    if (user is null)
                    {
                        return Results.Unauthorized();
                    }

                    var expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);
                    var token = CreateJwtToken(user, expiresAtUtc, _jwtOptions);

                    return Results.Ok(
                        new MoongateHttpLoginResponse
                        {
                            AccessToken = token,
                            TokenType = "Bearer",
                            ExpiresAtUtc = expiresAtUtc,
                            AccountId = user.AccountId,
                            Username = user.Username,
                            Role = user.Role
                        }
                    );
                }
            );
        }

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

    private static string BuildPrometheusPayload(MoongateHttpMetricsSnapshot snapshot)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# generated by moongate");
        sb.Append("# collected_at_unix_ms ")
          .AppendLine(snapshot.CollectedAt.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture));
        sb.AppendLine();

        var groupedMetrics = snapshot.Metrics
            .GroupBy(static pair => NormalizeMetricName(pair.Key))
            .OrderBy(static g => g.Key, StringComparer.Ordinal);

        foreach (var metricGroup in groupedMetrics)
        {
            var firstMetric = metricGroup.First();
            var metricType = firstMetric.Value.Type;
            var helpText = firstMetric.Value.Help ?? $"Moongate {metricGroup.Key} metric";

            sb.Append("# HELP moongate_")
              .Append(metricGroup.Key)
              .Append(' ')
              .AppendLine(helpText);

            sb.Append("# TYPE moongate_")
              .Append(metricGroup.Key)
              .Append(' ')
              .AppendLine(GetPrometheusTypeName(metricType));

            foreach (var (metricKey, metric) in metricGroup.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
            {
                sb.Append("moongate_")
                  .Append(metricGroup.Key);

                if (metric.Tags is not null && metric.Tags.Count > 0)
                {
                    sb.Append('{');
                    var firstLabel = true;
                    foreach (var (labelKey, labelValue) in metric.Tags.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
                    {
                        if (!firstLabel)
                        {
                            sb.Append(',');
                        }

                        sb.Append(NormalizeLabelName(labelKey))
                          .Append("=\"")
                          .Append(EscapeLabelValue(labelValue))
                          .Append('"');
                        firstLabel = false;
                    }

                    sb.Append('}');
                }

                sb.Append(' ')
                  .Append(metric.Value.ToString(CultureInfo.InvariantCulture));

                if (metric.Timestamp.HasValue)
                {
                    sb.Append(' ')
                      .Append(metric.Timestamp.Value.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture));
                }

                sb.AppendLine();
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string GetPrometheusTypeName(MetricType metricType) => metricType switch
    {
        MetricType.Counter => "counter",
        MetricType.Gauge => "gauge",
        MetricType.Histogram => "histogram",
        _ => "untyped",
    };

    private static string NormalizeLabelName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        var buffer = new char[value.Length];

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            buffer[i] = (i == 0 && !char.IsLetter(c)) || (!char.IsLetterOrDigit(c) && c != '_') ? '_' : char.ToLowerInvariant(c);
        }

        return new(buffer);
    }

    private static string EscapeLabelValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n");
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

        return new(buffer);
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

    private static void ConfigureJwt(IServiceCollection services, MoongateHttpJwtOptions options)
    {
        var keyBytes = Encoding.UTF8.GetBytes(options.SigningKey);
        var key = new SymmetricSecurityKey(keyBytes);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(
                jwtOptions =>
                {
                    jwtOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = options.Issuer,
                        ValidAudience = options.Audience,
                        IssuerSigningKey = key,
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
                }
            );

        services.AddAuthorization();
    }

    private static string CreateJwtToken(
        MoongateHttpAuthenticatedUser user,
        DateTimeOffset expiresAtUtc,
        MoongateHttpJwtOptions options
    )
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Username),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role),
            new("account_id", user.AccountId)
        };

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)),
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
