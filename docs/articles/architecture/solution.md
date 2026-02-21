# Solution Structure

Current project layout and responsibilities.

## Solution Tree

```
moongatev2/
├── src/
│   ├── Moongate.Abstractions
│   ├── Moongate.Core
│   ├── Moongate.Network
│   ├── Moongate.Network.Packets
│   ├── Moongate.Network.Packets.Generators
│   ├── Moongate.Persistence
│   ├── Moongate.Scripting
│   ├── Moongate.Server
│   ├── Moongate.Server.Http
│   ├── Moongate.Server.Metrics
│   ├── Moongate.Server.Metrics.Generators
│   └── Moongate.UO.Data
├── tests/
│   └── Moongate.Tests
├── docs/
├── scripts/
└── stack/
```

## Module Summary

- `Moongate.Abstractions`
  - base service abstractions shared across modules.
- `Moongate.Core`
  - shared types (`Serial`, geometry, json helpers, utility classes).
- `Moongate.Network`
  - TCP transport, spans, network middleware pipeline.
- `Moongate.Network.Packets`
  - packet contracts and concrete packet implementations.
- `Moongate.Network.Packets.Generators`
  - source generation for packet table/definitions.
- `Moongate.Persistence`
  - snapshot/journal storage and repositories.
- `Moongate.Scripting`
  - Lua engine and module bridge.
- `Moongate.Server`
  - composition root, bootstrap, runtime services and handlers.
- `Moongate.Server.Http`
  - HTTP service host and metric exposure endpoints.
- `Moongate.Server.Metrics`
  - metrics provider abstractions and snapshot collection.
- `Moongate.Server.Metrics.Generators`
  - generated metric mapping utilities.
- `Moongate.UO.Data`
  - UO entities, enums, templates, gameplay data contracts.

## Runtime Composition

`MoongateBootstrap` in `Moongate.Server` wires all services via DryIoc, then starts services ordered by registration priority.

Startup also handles:

- directory initialization
- logger setup
- config load/merge
- UO directory validation
- data asset copy bootstrap
- packet listener registration

---

**Previous**: [Architecture TOC](toc.yml) | **Next**: [Architecture Overview](overview.md)
