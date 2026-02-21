# Quick Start Guide

Get Moongate v2 up and running in 5 minutes.

## Prerequisites

Before you begin, ensure you have:

- **.NET SDK 10.0.x** - [Download from Microsoft](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Git** - For cloning the repository
- **Ultima Online Classic Client** - Version 7.x (for testing)

## Step 1: Clone the Repository

```bash
git clone https://github.com/moongate-community/moongatev2.git
cd moongatev2
```

## Step 2: Build the Project

```bash
# Restore dependencies
dotnet restore

# Build in Release configuration
dotnet build -c Release
```

## Step 3: Run the Server

```bash
# Run directly from source
dotnet run --project src/Moongate.Server
```

You should see output similar to:

```
                                                   __
 /'\_/`\                                          /\ \__
/\      \    ___     ___     ___      __      __  \ \ ,_\    __
\ \ \__\ \  / __`\  / __`\ /' _ `\  /'_ `\  /'__`\ \ \ \/  /'__`\
 \ \ \_/\ \/\ \L\ \/\ \L\ \/\ \/\ \/\ \L\ \/\ \L\.\_\ \ \_/\  __/
  \ \_\\ \_\ \____/\ \____/\ \_\ \_\ \____ \ \__/.\_\\ \__\ \____\
   \/_/ \/_/\/___/  \/___/  \/_/\/_/\/___L\ \/__/\/_/ \/__/\/____/
                                      /\____/
                                      \_/__/

Platform: MacOS (ARM64)
Is running from Docker: False

[HH:MM:SS DBG] Lua engine initialized
[HH:MM:SS INF] Moongate HTTP service started on port 8088
[HH:MM:SS INF] Server started in XXX.XX ms
[HH:MM:SS INF] Moongate server is running. Press Ctrl+C to stop.

moongate>
```

## Step 4: Verify the Server

### Check HTTP Endpoints

Open your browser and visit:

- **Health Check**: http://localhost:8088/health
- **Metrics**: http://localhost:8088/metrics
- **OpenAPI Docs**: http://localhost:8088/scalar

### Check Console

The interactive console prompt `moongate>` indicates the server is ready. Try these commands:

```
moongate> status
moongate> metrics
moongate> help
```

## Step 5: Connect with UO Client

> **Note**: Full client connectivity is still under development. Basic connection testing is possible once character creation is implemented.

1. Configure your UO client to connect to `localhost:2593`
2. Launch the UO client
3. Attempt to connect to your server

## Configuration

### Basic Settings

Create or edit `moongate.json` in the server directory:

```json
{
  "network": {
    "port": 2593,
    "logPackets": false
  },
  "http": {
    "enabled": true,
    "port": 8088,
    "openApiEnabled": true
  },
  "game": {
    "idleCpuEnabled": true,
    "idleSleepMilliseconds": 5
  },
  "directories": {
    "root": "/path/to/moongate-data",
    "uo": "/path/to/uo-client"
  }
}
```

### Environment Variables

You can also configure via environment variables:

```bash
export MOONGATE_ROOT_DIRECTORY=/path/to/moongate-data
export MOONGATE_UO_DIRECTORY=/path/to/uo-client
dotnet run --project src/Moongate.Server
```

## Running with Docker

### Build the Image

```bash
./scripts/build_image.sh -t moongate-server:local
```

### Run the Container

```bash
docker run --rm -it \
  -p 2593:2593 \
  -p 8088:8088 \
  -v /path/to/moongate-data:/app \
  -v /path/to/uo-client:/uo:ro \
  --name moongate \
  moongate-server:local
```

### Docker Compose (with Monitoring)

For a complete setup with Prometheus and Grafana:

```bash
cd stack
docker compose up -d --build
```

This starts:
- Moongate server
- Prometheus (scraping metrics every 5s)
- Grafana (pre-configured dashboard)

Access points:
- **Grafana**: http://localhost:3000 (admin/admin)
- **Prometheus**: http://localhost:9090
- **Moongate Metrics**: http://localhost:8088/metrics

## Troubleshooting

### Port Already in Use

```bash
# Check what's using the port
lsof -i :2593
lsof -i :8088

# Kill the process or change the port in configuration
```

### .NET SDK Not Found

```bash
# Verify .NET installation
dotnet --version

# Should show 10.0.x
# If not, install from https://dotnet.microsoft.com/download/dotnet/10.0
```

### Build Errors

```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build -c Release
```

### Docker Permission Issues

```bash
# Ensure directories are writable
chmod -R 755 /path/to/moongate-data

# Run Docker with proper user mapping
docker run --user $(id -u):$(id -g) ...
```

## Next Steps

Now that you have Moongate v2 running:

1. **[Configuration Guide](configuration.md)** - Detailed configuration options
2. **[Architecture Overview](../architecture/overview.md)** - Understand the internals
3. **[Scripting Guide](../scripting/overview.md)** - Extend with Lua scripts
4. **[Persistence Guide](../persistence/overview.md)** - Data storage details

## Useful Commands

```bash
# Run tests
dotnet test

# Build for production (NativeAOT)
dotnet publish -c Release -r linux-x64 --self-contained

# View logs
docker logs -f moongate

# Stop server
docker compose down
```

## Getting Help

- **Documentation**: Browse the [API Reference](../api/index.md)
- **Issues**: Report bugs on [GitHub Issues](https://github.com/moongate-community/moongatev2/issues)
- **Discord**: Join our [community server](https://discord.gg/3HT7v95b)

---

**Previous**: [Introduction](introduction.md) | **Next**: [Installation Guide](installation.md)
