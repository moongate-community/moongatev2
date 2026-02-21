# Moongate Monitoring Stack

Docker Compose stack for monitoring Moongate server with Prometheus and Grafana.

## Quick Start

### 1. Build and start the entire stack

```bash
cd stack
docker compose up -d --build
```

This will:
- Build the Moongate server image from the repository root (NativeAOT)
- Start Moongate, Prometheus, and Grafana

### 2. Access the services

- **Grafana**: http://localhost:3000
  - Username: `admin`
  - Password: `admin`
  - Dashboard: "Moongate Server Metrics"

- **Prometheus**: http://localhost:9090
  - Targets: http://localhost:9090/targets

- **Moongate Health**: http://localhost:8088/health
- **Moongate Metrics**: http://localhost:8088/metrics
- **Moongate OpenAPI/Scalar**: http://localhost:8088/scalar

### 3. Stop the stack

```bash
cd stack
docker compose down
```

## Configuration

### Directory Configuration

The stack is configured with the following host directories:

- **Moongate Root**: `/Users/squid/moongate` → `/app/data`, `/app/save`, `/app/scripts`, `/app/templates`, `/app/logs` (read-write)
- **Ultima Online Client**: `/Users/squid/uo` → `/uo` (read-only)

To change these paths, edit the `volumes` section in `docker-compose.yml`:

```yaml
volumes:
  - /your/path/moongate/data:/app/data
  - /your/path/moongate/save:/app/save
  - /your/path/moongate/scripts:/app/scripts
  - /your/path/moongate/templates:/app/templates
  - /your/path/moongate/logs:/app/logs
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
- Dashboard refresh: every 5 seconds

## Metrics Exposed

Moongate exposes the following metrics at `:8088/metrics`:

### Game Loop
- `moongate_gameloop_tick_duration_avg_ms` - Average tick duration in milliseconds
- `moongate_gameloop_tick_duration_max_ms` - Maximum tick duration in milliseconds
- `moongate_gameloop_loop_work_units_avg` - Average work units per loop
- `moongate_gameloop_loop_tick_count` - Total tick count
- `moongate_gameloop_loop_idle_sleep_count` - Number of idle sleeps
- `moongate_gameloop_ticks_total` - Total game loop ticks executed

### Network
- `moongate_gameloop_network_outbound_packets_total` - Total outbound packets processed
- `moongate_gameloop_network_outbound_queue_depth` - Current outbound queue depth
- `moongate_network_network_inbound_packets_total` - Total inbound packets received
- `moongate_network_network_inbound_queue_depth` - Current inbound queue depth
- `moongate_network_network_inbound_unknown_opcode_total` - Unknown opcode count

### Scripting
- `moongate_scripting_cache_entries_total` - Number of script cache entries
- `moongate_scripting_cache_hits_total` - Script cache hits
- `moongate_scripting_cache_misses_total` - Script cache misses
- `moongate_scripting_execution_time_ms` - Script execution time in milliseconds
- `moongate_scripting_memory_used_bytes` - Memory used by scripting engine
- `moongate_scripting_statements_executed` - Total statements executed

### Timer
- `moongate_timer_timer_processed_ticks_total` - Total timer ticks processed

## Grafana Dashboard Panels

The auto-provisioned dashboard includes:

1. **Game Loop Tick Duration** - Avg/Max tick duration over time (line chart)
2. **Game Loop Work Units** - Average work units per loop (line chart)
3. **Network Outbound Queue** - Outbound queue depth over time (line chart)
4. **Network Inbound Queue** - Inbound queue depth over time (line chart)
5. **Idle Sleep Count** - CPU idle sleep count over time (line chart)
6. **Avg Tick Duration** - Current average tick duration (stat panel)
7. **Outbound Packets Total** - Total outbound packets (stat panel)
8. **Total Game Ticks** - Total game loop ticks executed (stat panel)
9. **Script Cache Entries** - Number of script cache entries (stat panel)

## Commands

```bash
# Build and start stack
docker compose up -d --build

# Start stack (without rebuild)
docker compose up -d

# Stop stack
docker compose down

# View all logs
docker compose logs -f

# View Moongate logs only
docker compose logs -f moongate

# View Prometheus logs only
docker compose logs -f prometheus

# View Grafana logs only
docker compose logs -f grafana

# Rebuild Moongate after code changes
docker compose up -d --build moongate

# Restart a specific service
docker compose restart <service-name>

# Reset all data (volumes)
docker compose down -v

# Reset only Grafana data (dashboards will be re-provisioned)
docker compose down -v grafana_data
docker compose up -d grafana

# Reset only Prometheus data (metrics history will be lost)
docker compose down -v prometheus_data
docker compose up -d prometheus
```

## Architecture

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   Moongate      │────▶│   Prometheus    │────▶│    Grafana      │
│   Server        │     │   (scrape :05s) │     │   (dashboard)   │
│   :8088/metrics │     │   :9090         │     │   :3000         │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

All services run on the `moongate-network` Docker network for internal communication.

## Troubleshooting

### Prometheus can't scrape metrics

1. Ensure Moongate container is running: `docker compose ps`
2. Check Moongate logs: `docker compose logs moongate`
3. Verify metrics endpoint: `curl http://localhost:8088/metrics`
4. Check Prometheus targets: http://localhost:9090/targets (should show "UP" in green)

### Moongate fails to build

1. Ensure .NET 10 SDK is available in the Docker build
2. Check build logs: `docker compose build moongate --progress=plain`
3. Clean build cache: `docker compose build --no-cache moongate`
4. Verify Docker has enough memory allocated (minimum 4GB recommended for NativeAOT)

### Grafana shows no data

1. Check Prometheus datasource is configured (Configuration → Data Sources → Prometheus)
2. Verify Prometheus is scraping successfully (Status → Targets in Prometheus UI)
3. Wait a few seconds for data to be collected (scrape interval is 5s)
4. Check the time range selector in Grafana (should be set to last 15 minutes by default)

### Dashboard shows "No data" for specific panels

1. Verify the metric exists: `curl http://localhost:8088/metrics | grep <metric-name>`
2. Some metrics only appear after certain actions (e.g., network metrics after client connections)
3. Check Prometheus query in Explore mode to verify the metric is being collected

### Container won't start

1. Check for port conflicts: `docker compose ps` and `lsof -i :8088`
2. Verify host directories exist and have correct permissions
3. Check Docker daemon is running: `docker info`

### Moongate server crashes

1. Check logs: `docker compose logs --tail=100 moongate`
2. Verify UO client files are accessible in `/Users/squid/uo`
3. Ensure save/data directories are writable

## Development Workflow

### Making code changes

```bash
# 1. Edit code in src/
# 2. Rebuild and restart Moongate container
docker compose up -d --build moongate

# 3. Watch logs
docker compose logs -f moongate

# 4. Check metrics
curl http://localhost:8088/metrics | grep <your-metric>
```

### Adding new metrics

1. Add metric in `src/Moongate.Server.Metrics/`
2. Rebuild: `docker compose up -d --build moongate`
3. Verify: `curl http://localhost:8088/metrics`
4. Add panel to `grafana/provisioning/dashboards/moongate-metrics.json`
5. Restart Grafana: `docker compose restart grafana`

## License

This project is licensed under the GNU General Public License v3.0.
