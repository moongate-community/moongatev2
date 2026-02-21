# Quick Start Guide

Get Moongate v2 running locally with the current server behavior.

## Prerequisites

- .NET SDK 10.0.x
- Ultima Online client data directory
- Git

## 1. Clone

```bash
git clone https://github.com/moongate-community/moongatev2.git
cd moongatev2
```

## 2. Build

```bash
dotnet restore
dotnet build -c Release
```

## 3. Run

You must provide UO directory (CLI or env var).

```bash
dotnet run --project src/Moongate.Server -- \
  --rootDirectory ./moongate \
  --uoDirectory /path/to/uo
```

Or with env var:

```bash
export MOONGATE_UO_DIRECTORY=/path/to/uo
dotnet run --project src/Moongate.Server
```

## 4. Verify

HTTP checks:

- `http://localhost:8088/health` → `ok`
- `http://localhost:8088/metrics` → Prometheus payload (or config message)
- `http://localhost:8088/scalar` → OpenAPI UI (if enabled)

## 5. Console Commands

Built-in default commands currently include:

- `help`
- `lock`
- `exit`

## 6. Docker (optional)

Build image:

```bash
./scripts/build_image.sh -t moongate-server:local
```

Run:

```bash
docker run --rm -it \
  -p 2593:2593 \
  -p 8088:8088 \
  -v /path/to/moongate-data:/app \
  -v /path/to/uo:/uo:ro \
  --name moongate \
  moongate-server:local
```

## Troubleshooting

- If startup fails with UO directory error, set `--uoDirectory` or `MOONGATE_UO_DIRECTORY`.
- If ports are busy, stop conflicting process or remap ports.

---

**Previous**: [Introduction](introduction.md) | **Next**: [Installation Guide](installation.md)
