# Protocol Reference

Reference for current packet handling behavior in Moongate v2.

## Packet Framing

- First byte: opcode
- Fixed packet: total length from registry descriptor
- Variable packet: bytes `[1..2]` are big-endian length including header

## Parsing Rules

`NetworkService` enforces:

- descriptor must exist for opcode
- enough bytes must be buffered for full packet
- variable declared length must be within allowed bounds
- `TryParse` must succeed

On repeated violations, session is disconnected.

## Selected Inbound Packets

- `0xEF` Login Seed (`Length=21`, fixed)
- `0x80` Account Login (`Length=62`, fixed)
- `0xA0` Server Select (`Length=3`, fixed)
- `0x91` Game Login (`Length=65`, fixed)
- `0x5D` Login Character (`Length=73`, fixed)
- `0xF8` Character Creation (`Length=106`, fixed)
- `0x02` Move Request (`Length=7`, fixed)
- `0x07` Pick Up (`Length=7`, fixed)
- `0x08` Drop Item (`Length=14`, fixed)
- `0x34` Get Player Status (`Length=10`, fixed)
- `0x72` Request War Mode (`Length=5`, fixed)
- `0x73` Ping Message (`Length=2`, fixed)
- `0xAD` Unicode Speech (`variable`)
- `0xBF` General Information (`variable`)

## Selected Outbound Packets

- `0xA8` Server List (variable)
- `0x8C` Server Redirect (`Length=11`, fixed)
- `0x1B` Login Confirm (`Length=37`, fixed)
- `0xA9` Character / Starting Locations (variable)
- `0xB9` Support Features (`Length=5`, fixed)
- `0x55` Login Complete (`Length=1`, fixed)
- `0x78` Mobile Incoming (variable)
- `0x88` Paperdoll (`Length=66`, fixed)
- `0x11` Player Status (`Length=43`, fixed)
- `0xAE` Unicode Speech Message (variable)

## Opcode Constants

`PacketDefinition` is generated as a `partial` static class and used by bootstrap/handlers to avoid hardcoded byte literals.

## Notes

- Length/source metadata is defined in packet attributes and registration.
- Runtime listener availability is independent from packet registration: a packet can be parseable but have no listener yet.

---

**Previous**: [Packet System](packets.md)
