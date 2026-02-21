# Configuration Guide

Comprehensive guide to configuring Moongate v2.

## Configuration Methods

Moongate v2 supports multiple configuration methods:

1. **Command-line arguments** (highest priority)
2. **Environment variables**
3. **Configuration file** (`moongate.json`)
4. **Default values** (lowest priority)

## Configuration File

### Location

The configuration file `moongate.json` is located in:

- **Development**: Project root directory
- **Docker**: `/app/moongate.json`
- **Custom**: Specified via `--rootDirectory` argument

### Example Configuration

```json
{
  "network": {
    "port": 2593,
    "logPackets": false,
    "maxConnections": 100
  },
  "http": {
    "enabled": true,
    "port": 8088,
    "openApiEnabled": true
  },
  "game": {
    "idleCpuEnabled": true,
    "idleSleepMilliseconds": 5,
    "tickRateMilliseconds": 100
  },
  "logging": {
    "level": "Debug",
    "logPacketData": true,
    "consoleEnabled": true,
    "fileEnabled": true
  },
  "directories": {
    "root": "/opt/moongate",
    "uo": "/uo"
  },
  "persistence": {
    "enabled": true,
    "snapshotIntervalMinutes": 5,
    "journalFlushIntervalSeconds": 30
  },
  "scripting": {
    "enabled": true,
    "scriptsDirectory": "scripts",
    "autoReload": false
  }
}
```

## Configuration Sections

### Network Configuration

```json
{
  "network": {
    "port": 2593,                    // TCP port for UO client connections
    "logPackets": false,             // Log all packet data (verbose)
    "maxConnections": 100,           // Maximum concurrent connections
    "receiveBufferSize": 8192,       // Socket receive buffer size
    "sendBufferSize": 8192           // Socket send buffer size
  }
}
```

### HTTP Configuration

```json
{
  "http": {
    "enabled": true,                 // Enable HTTP server
    "port": 8088,                    // HTTP server port
    "openApiEnabled": true,          // Enable OpenAPI/Scalar documentation
    "metricsEnabled": true,          // Enable Prometheus metrics endpoint
    "corsEnabled": false             // Enable CORS (for web clients)
  }
}
```

### Game Loop Configuration

```json
{
  "game": {
    "idleCpuEnabled": true,          // Enable CPU throttling when idle
    "idleSleepMilliseconds": 5,      // Sleep duration when idle
    "tickRateMilliseconds": 100,     // Target tick duration
    "maxTickDurationMilliseconds": 500  // Maximum allowed tick duration
  }
}
```

### Logging Configuration

```json
{
  "logging": {
    "level": "Debug",                // Minimum log level (Debug, Info, Warning, Error)
    "logPacketData": true,           // Include packet data in logs
    "consoleEnabled": true,          // Enable console output
    "fileEnabled": true,             // Enable file logging
    "fileDirectory": "logs",         // Log file directory
    "fileRetentionDays": 30          // Days to retain log files
  }
}
```

### Directory Configuration

```json
{
  "directories": {
    "root": "/opt/moongate",         // Root directory for server data
    "uo": "/uo"                      // Ultima Online client directory
  }
}
```

Directory structure under `root`:

```
/opt/moongate/
├── data/          # Game data (auto-created)
├── logs/          # Log files (auto-created)
├── save/          # Persistence snapshots and journals (auto-created)
├── scripts/       # Lua scripts (auto-created)
└── templates/     # Item and mobile templates (auto-created)
```

### Persistence Configuration

```json
{
  "persistence": {
    "enabled": true,                 // Enable persistence system
    "snapshotIntervalMinutes": 5,    // Time between automatic snapshots
    "journalFlushIntervalSeconds": 30,  // Journal flush interval
    "compressionEnabled": true       // Compress snapshot files
  }
}
```

### Scripting Configuration

```json
{
  "scripting": {
    "enabled": true,                 // Enable Lua scripting
    "scriptsDirectory": "scripts",   // Directory containing Lua scripts
    "autoReload": false,             // Auto-reload scripts on change (development)
    "debugMode": false               // Enable script debugging
  }
}
```

## Environment Variables

All configuration options can be set via environment variables:

```bash
# Network
export MOONGATE_NETWORK_PORT=2593
export MOONGATE_NETWORK_LOGPACKETS=false

# HTTP
export MOONGATE_HTTP_ENABLED=true
export MOONGATE_HTTP_PORT=8088
export MOONGATE_HTTP_OPENAPIENABLED=true

# Game
export MOONGATE_GAME_IDLECPUENABLED=true
export MOONGATE_GAME_IDLESLEEP MILLISECONDS=5

# Directories
export MOONGATE_ROOT_DIRECTORY=/opt/moongate
export MOONGATE_UO_DIRECTORY=/uo

# Logging
export MOONGATE_LOGGING_LEVEL=Debug
export MOONGATE_LOGGING_LOGPACKETDATA=true
```

## Command-Line Arguments

Run `dotnet run --project src/Moongate.Server -- --help` to see all options:

```bash
# Run with custom configuration
dotnet run --project src/Moongate.Server -- \
  --rootDirectory /opt/moongate \
  --uoDirectory /uo \
  --loglevel Debug \
  --showHeader true
```

Available arguments:

| Argument | Type | Default | Description |
|----------|------|---------|-------------|
| `--rootDirectory` | string | `.` | Root directory for server data |
| `--uoDirectory` | string | `./uo` | Ultima Online client directory |
| `--loglevel` | enum | `Debug` | Logging level (Debug, Info, Warning, Error) |
| `--showHeader` | bool | `true` | Show startup banner |

## Docker Configuration

### Environment Variables in Docker

```yaml
services:
  moongate:
    image: moongate-server:latest
    environment:
      - MOONGATE_ROOT_DIRECTORY=/app
      - MOONGATE_UO_DIRECTORY=/uo
      - MOONGATE_HTTP_PORT=8088
      - MOONGATE_NETWORK_PORT=2593
    volumes:
      - /opt/moongate/data:/app/data
      - /opt/moongate/logs:/app/logs
      - /opt/moongate/save:/app/save
      - /path/to/uo:/uo:ro
```

### Docker Compose Full Example

```yaml
services:
  moongate:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: moongate-server
    environment:
      - MOONGATE_ROOT_DIRECTORY=/app
      - MOONGATE_UO_DIRECTORY=/uo
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8088
    volumes:
      - /opt/moongate/data:/app/data
      - /opt/moongate/logs:/app/logs
      - /opt/moongate/save:/app/save
      - /opt/moongate/scripts:/app/scripts
      - /opt/moongate/templates:/app/templates
      - /path/to/uo-client:/uo:ro
    ports:
      - "2593:2593"
      - "8088:8088"
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "wget", "-q", "--spider", "http://localhost:8088/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 60s
```

## Configuration Best Practices

### Development

```json
{
  "logging": {
    "level": "Debug",
    "logPacketData": true
  },
  "game": {
    "idleCpuEnabled": false
  },
  "scripting": {
    "autoReload": true,
    "debugMode": true
  }
}
```

### Production

```json
{
  "logging": {
    "level": "Information",
    "logPacketData": false,
    "fileRetentionDays": 90
  },
  "game": {
    "idleCpuEnabled": true,
    "idleSleepMilliseconds": 5
  },
  "persistence": {
    "snapshotIntervalMinutes": 5,
    "compressionEnabled": true
  },
  "scripting": {
    "autoReload": false,
    "debugMode": false
  }
}
```

### High-Performance

```json
{
  "game": {
    "idleCpuEnabled": false,
    "tickRateMilliseconds": 50
  },
  "network": {
    "receiveBufferSize": 16384,
    "sendBufferSize": 16384
  },
  "persistence": {
    "journalFlushIntervalSeconds": 60
  }
}
```

## Validation

The server validates configuration on startup. Invalid configurations will prevent the server from starting with clear error messages.

### Common Validation Errors

```
Error: HTTP port 8088 is already in use.
Solution: Change the port or stop the conflicting service.

Error: UO directory '/uo' does not exist or is not accessible.
Solution: Verify the path and permissions.

Error: Invalid log level 'Verbose'. Valid values: Debug, Info, Warning, Error.
Solution: Use a valid log level.
```

## Reloading Configuration

### Development

Restart the server to apply configuration changes:

```bash
# Stop server (Ctrl+C)
# Start server
dotnet run --project src/Moongate.Server
```

### Production (Docker)

```bash
# Update configuration file
# Restart container
docker compose restart moongate
```

## Troubleshooting

### Configuration Not Applied

1. Verify file location and name (`moongate.json`)
2. Check JSON syntax (use a JSON validator)
3. Verify environment variables aren't overriding
4. Check startup logs for configuration summary

### Port Conflicts

```bash
# Check what's using the port
lsof -i :8088
netstat -tlnp | grep 8088

# Change port in configuration
{
  "http": {
    "port": 8089
  }
}
```

### Permission Issues

```bash
# Linux/Docker: Ensure directories are writable
chmod -R 755 /opt/moongate
chown -R $(id -u):$(id -g) /opt/moongate
```

## Next Steps

- **[Architecture Overview](../architecture/overview.md)** - Understand the system design
- **[Scripting Guide](../scripting/overview.md)** - Extend with Lua
- **[Persistence Guide](../persistence/overview.md)** - Data storage details

---

**Previous**: [Installation](installation.md) | **Next**: [Architecture Overview](../architecture/overview.md)
