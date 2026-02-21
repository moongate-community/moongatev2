# Configuration Guide

This guide reflects the current runtime behavior in Moongate v2.

## Configuration Sources

Priority order:

1. Command-line arguments
2. Environment variables
3. `moongate.json` in root directory
4. Defaults

## Command-Line Arguments

Current `Program` command signature supports:

- `--showHeader` (`bool`, default `true`)
- `--rootDirectory` (`string`)
- `--uoDirectory` (`string`)
- `--loglevel` (`LogLevelType`, default `Debug`)

Example:

```bash
dotnet run --project src/Moongate.Server -- \
  --rootDirectory /opt/moongate \
  --uoDirectory /opt/uo \
  --loglevel Information
```

## Environment Variables

Currently used by bootstrap/runtime:

- `MOONGATE_ROOT_DIRECTORY`
- `MOONGATE_UO_DIRECTORY`
- `MOONGATE_ADMIN_USERNAME`
- `MOONGATE_ADMIN_PASSWORD`

`MOONGATE_ROOT_DIRECTORY` is used if `--rootDirectory` is not passed.

`MOONGATE_UO_DIRECTORY` is required if `--uoDirectory` is not passed.

## `moongate.json`

Location:

- `<RootDirectory>/moongate.json`

If missing, bootstrap creates one with default values.

### Important Current Limitation

At the moment, bootstrap explicitly applies these fields from file:

- `RootDirectory`
- `UODirectory`
- `LogLevel`
- `LogPacketData`
- `Persistence`

Other sections exist in the config model (`Http`, `Game`, `Metrics`), but are not explicitly merged in `CheckConfig()` yet.

## Config Model

Top-level shape:

```json
{
  "rootDirectory": "/opt/moongate",
  "uoDirectory": "/opt/uo",
  "logLevel": "Information",
  "logPacketData": true,
  "isDeveloperMode": false,
  "http": {
    "isEnabled": true,
    "port": 8088,
    "isOpenApiEnabled": true
  },
  "game": {
    "shardName": "Moongate Shard",
    "timerTickMilliseconds": 250,
    "timerWheelSize": 512,
    "idleCpuEnabled": true,
    "idleSleepMilliseconds": 1
  },
  "metrics": {
    "enabled": true,
    "intervalMilliseconds": 1000,
    "logEnabled": true,
    "logToConsole": false,
    "logLevel": "Trace"
  },
  "persistence": {
    "saveIntervalSeconds": 30
  }
}
```

## Directories

`DirectoriesConfig` auto-creates directory tree under root using `DirectoryType` values:

- `data`
- `templates`
- `scripts`
- `save`
- `logs`
- `cache`
- `database`

## HTTP Endpoints

When HTTP is enabled:

- `/` → plain text service banner
- `/health` → plain text `ok`
- `/metrics` → Prometheus text format (if metrics factory configured)
- `/scalar` and `/openapi/*` (if OpenAPI enabled)

## Persistence Setting

Only persistence knob currently exposed:

- `Persistence.SaveIntervalSeconds` (autosave interval)

## Docker Notes

For container runs, typical environment:

```bash
MOONGATE_ROOT_DIRECTORY=/app
MOONGATE_UO_DIRECTORY=/uo
```

Mount `/app` for runtime data and `/uo` for UO client files.

---

**Previous**: [Installation Guide](installation.md) | **Next**: [Quick Start](quickstart.md)
