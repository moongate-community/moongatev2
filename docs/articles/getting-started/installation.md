# Installation Guide

Detailed installation instructions for Moongate v2 on different platforms.

## System Requirements

### Minimum Requirements

- **CPU**: Dual-core 2.0 GHz or equivalent
- **RAM**: 4 GB
- **Storage**: 2 GB available space
- **OS**: Windows 10, macOS 11+, or Linux (Ubuntu 20.04+)

### Recommended Requirements

- **CPU**: Quad-core 3.0 GHz or equivalent
- **RAM**: 8 GB
- **Storage**: 5 GB available space (SSD preferred)
- **OS**: Latest stable version of your preferred OS

## Platform-Specific Installation

### Windows

#### 1. Install .NET SDK

Download and install [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) for Windows.

Verify installation:

```powershell
dotnet --version
```

#### 2. Install Git

Download [Git for Windows](https://git-scm.com/download/win).

#### 3. Clone and Build

```powershell
git clone https://github.com/moongate-community/moongatev2.git
cd moongatev2
dotnet restore
dotnet build -c Release
```

#### 4. Run the Server

```powershell
dotnet run --project src/Moongate.Server
```

### macOS

#### 1. Install .NET SDK

Using Homebrew (recommended):

```bash
brew install --cask dotnet-sdk
```

Or download from [Microsoft](https://dotnet.microsoft.com/download/dotnet/10.0).

Verify installation:

```bash
dotnet --version
```

#### 2. Install Git

Git is pre-installed on macOS. Verify:

```bash
git --version
```

#### 3. Clone and Build

```bash
git clone https://github.com/moongate-community/moongatev2.git
cd moongatev2
dotnet restore
dotnet build -c Release
```

#### 4. Run the Server

```bash
dotnet run --project src/Moongate.Server
```

### Linux (Ubuntu/Debian)

#### 1. Install .NET SDK

```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install .NET SDK 10.0
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0
```

Verify installation:

```bash
dotnet --version
```

#### 2. Install Git

```bash
sudo apt-get install -y git
```

#### 3. Clone and Build

```bash
git clone https://github.com/moongate-community/moongatev2.git
cd moongatev2
dotnet restore
dotnet build -c Release
```

#### 4. Run the Server

```bash
dotnet run --project src/Moongate.Server
```

### Linux (NativeAOT Production Build)

For production deployment with NativeAOT:

```bash
# Install build dependencies
sudo apt-get install -y clang zlib1g-dev

# Publish NativeAOT binary
dotnet publish src/Moongate.Server/Moongate.Server.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -p:StripSymbols=true

# Run the published binary
./src/Moongate.Server/bin/Release/net10.0/linux-x64/publish/Moongate.Server
```

## Docker Installation

### Prerequisites

- Docker 20.10+
- Docker Compose 2.0+

### Quick Setup

```bash
# Clone repository
git clone https://github.com/moongate-community/moongatev2.git
cd moongatev2

# Build Docker image
./scripts/build_image.sh -t moongate-server:latest

# Create data directories
mkdir -p /opt/moongate/{data,logs,save,scripts,templates}

# Run container
docker run -d \
  --name moongate \
  -p 2593:2593 \
  -p 8088:8088 \
  -v /opt/moongate/data:/app/data \
  -v /opt/moongate/logs:/app/logs \
  -v /opt/moongate/save:/app/save \
  -v /opt/moongate/scripts:/app/scripts \
  -v /opt/moongate/templates:/app/templates \
  -v /path/to/uo-client:/uo:ro \
  moongate-server:latest
```

### Docker Compose (with Monitoring)

```bash
cd stack
docker compose up -d --build
```

This deploys:
- Moongate server
- Prometheus for metrics collection
- Grafana for visualization

## Ultima Online Client Setup

### Obtaining UO Client

You need a legitimate Ultima Online Classic Client (version 7.x).

### Configuring Client Path

Set the UO client directory:

**Via environment variable:**
```bash
export MOONGATE_UO_DIRECTORY=/path/to/uo-client
```

**Via configuration file:**
```json
{
  "directories": {
    "uo": "/path/to/uo-client"
  }
}
```

**Via command line:**
```bash
dotnet run --project src/Moongate.Server -- --uoDirectory /path/to/uo-client
```

### Required Client Files

The following files should be present in your UO directory:

- `client.exe` or `client.dll`
- `UO3D.exe` (for 3D client)
- `maps/` directory with map files
- `artidx.mul` and `art.mul`
- `tileidx.mul` and `tiledata.mul`

## Post-Installation Verification

### 1. Check Server Health

```bash
curl http://localhost:8088/health
```

Expected response:
```json
{"status":"Healthy"}
```

### 2. Check Metrics Endpoint

```bash
curl http://localhost:8088/metrics
```

Should return Prometheus-format metrics.

### 3. Check OpenAPI Documentation

Open in browser: http://localhost:8088/scalar

### 4. Verify Console UI

The server should display:
```
moongate>
```

Type `help` to see available commands.

## Directory Structure

After installation, your directory structure should look like:

```
moongatev2/
├── src/                      # Source code
│   ├── Moongate.Server/     # Main server project
│   ├── Moongate.Network/    # Network layer
│   ├── Moongate.Persistence/# Data persistence
│   └── ...
├── tests/                    # Unit tests
├── docs/                     # Documentation
├── stack/                    # Docker Compose stack
├── scripts/                  # Build scripts
└── moongate.json            # Configuration (created on first run)
```

Data directories (created at runtime):

```
/app/ (or MOONGATE_ROOT_DIRECTORY)
├── data/          # Game data files
├── logs/          # Server logs
├── save/          # Persistence snapshots and journals
├── scripts/       # Lua scripts
└── templates/     # Item and mobile templates
```

## Troubleshooting

### .NET SDK Issues

**Problem**: `dotnet` command not found

```bash
# Verify PATH includes .NET
echo $PATH | grep dotnet

# On Linux, may need to add to ~/.bashrc
export PATH=$PATH:/usr/share/dotnet
```

### Build Failures

**Problem**: Restore or build errors

```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### Permission Issues (Linux/Docker)

```bash
# Set proper ownership
sudo chown -R $(id -u):$(id -g) /opt/moongate

# Or run Docker with current user
docker run --user $(id -u):$(id -g) ...
```

### Port Conflicts

```bash
# Check what's using ports
lsof -i :2593
lsof -i :8088

# Change ports in configuration if needed
```

## Next Steps

- **[Quick Start](quickstart.md)** - Get the server running
- **[Configuration](configuration.md)** - Customize your server
- **[Architecture](../architecture/overview.md)** - Understand the internals

---

**Previous**: [Quick Start](quickstart.md) | **Next**: [Configuration](configuration.md)
