# Moongate Monitoring Stack

Docker Compose stack for monitoring Moongate server with Prometheus and Grafana.

## Quick Start

### 1. Build and start the entire stack

```bash
cd stack
docker compose up -d --build
```

This will:
- Build the Moongate server image from the repository root
- Start Moongate, Prometheus, and Grafana

### 2. Access the services

- **Grafana**: http://localhost:3000
  - Username: `admin`
  - Password: `admin`
  - Dashboard: "Moongate Server Metrics"

- **Prometheus**: http://localhost:9090

- **Moongate Health**: http://localhost:8088/health
- **Moongate Metrics**: http://localhost:8088/metrics
- **Moongate OpenAPI**: http://localhost:8088/scalar

## Configuration

### Directory Configuration

The stack is configured with the following host directories:

- **Moongate Root**: `/Users/squid/moongate` → `/app` (read-write)
- **Ultima Online Client**: `/Users/squid/uo` → `/uo` (read-only)

To change these paths, edit the `volumes` section in `docker-compose.yml`:

```yaml
volumes:
  - /your/path/moongate:/app
  - /your/path/uo-client:/uo:ro
```

### Prometheus

Edit `prometheus/prometheus.yml` to change scrape intervals or add additional targets.

Default scrape config:
- Target: `moongate:8088` (internal Docker network)
- Metrics path: `/metrics`
- Scrape interval: 5s

### Grafana

- Default dashboard is auto-provisioned in `grafana/provisioning/dashboards/moongate-metrics.json`
- Datasource is auto-configured in `grafana/provisioning/datasources/datasources.yml`

## Metrics Exposed

Moongate exposes the following metrics at `:8088/metrics`:

- `process_cpu_seconds_total` - Total user and system CPU time spent in seconds
- `process_resident_memory_bytes` - Resident memory size in bytes
- `process_open_handles` - Number of open handles
- `timer_*` - Timer wheel metrics

## Commands

```bash
# Build and start stack
docker compose up -d --build

# Start stack (without rebuild)
docker compose up -d

# Stop stack
docker compose down

# View logs
docker compose logs -f

# View Moongate logs only
docker compose logs -f moongate

# Rebuild Moongate after code changes
docker compose up -d --build moongate

# Reset data (volumes)
docker compose down -v
```

## Troubleshooting

### Prometheus can't scrape metrics

1. Ensure Moongate container is healthy: `docker compose ps`
2. Check Moongate logs: `docker compose logs moongate`
3. Verify metrics endpoint: `curl http://localhost:8088/metrics`
4. Check Prometheus targets: http://localhost:9090/targets

### Moongate fails to build

1. Ensure .NET 10 SDK is available in the Docker build
2. Check build logs: `docker compose build moongate --progress=plain`
3. Clean build cache: `docker compose build --no-cache moongate`

### Grafana shows no data

1. Check Prometheus datasource is configured (Configuration > Data Sources > Prometheus)
2. Verify Prometheus is scraping successfully (Status > Targets in Prometheus UI)
3. Wait a few seconds for data to be collected

### Moongate health check fails

1. The health check has a 60s start period for NativeAOT cold start
2. Check logs: `docker compose logs moongate`
3. Verify HTTP service is enabled on port 8088
