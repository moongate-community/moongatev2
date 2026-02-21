# Data Format

This page documents the **actual** binary persistence format currently implemented in Moongate v2.

## Serialization Technology

- Serializer: `MemoryPack`
- Snapshot container: `WorldSnapshot`
- Journal payload item: `JournalEntry`

## Snapshot File

Path:

- `save/world.snapshot.bin`

Write behavior:

1. Serialize `WorldSnapshot` to `world.snapshot.bin.tmp`
2. Flush stream
3. Replace target file with atomic move

Read behavior:

- If snapshot file does not exist: returns `null`
- Otherwise: deserializes `WorldSnapshot` with MemoryPack

There is no trailing checksum block in the snapshot file.

## Journal File

Path:

- `save/world.journal.bin`

Each record layout in the file:

```
[int32 payloadLength LE]
[byte[payloadLength] payload]
[uint32 checksum LE]
```

Where:

- `payload` is `MemoryPackSerializer.Serialize(entry)`
- `checksum` is computed from `payload`

## Journal Validation Rules

During replay (`ReadAllAsync`):

- Record length must be present and valid
- Length must be in allowed bounds
- Payload must be fully readable
- Checksum must match
- Payload must deserialize to `JournalEntry`

Replay stops at first invalid/truncated record.

## Operational Lifecycle

- Repositories append operations to journal
- Unit of work builds fresh `WorldSnapshot` from state store
- Snapshot save completes
- Journal is reset (`FileMode.Create`)

## Persistence Options

`PersistenceOptions` currently includes exactly:

- `SnapshotFilePath`
- `JournalFilePath`

No extra sidecar/checksum/history paths are currently configured.

---

**Previous**: [Persistence Overview](overview.md) | **Next**: [Persistence Repositories](repositories.md)
