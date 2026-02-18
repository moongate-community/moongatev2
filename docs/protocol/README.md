# Protocol Notes

Store packet notes, handshake flow details, reverse engineering notes, and client/server references here.

## Implemented Handshake Notes

- Initial session state is `AwaitingSeed`.
- Login path:
  - first packet `0xEF` (`LoginSeedPacket`) then account login flow (`0x80`).
- Reconnect/game-server path:
  - client can send a raw 4-byte seed first.
  - seed is consumed and stored before normal packet parsing.
  - parser then expects standard login/game packets (`0x80` / `0x91`) according to flow.
- Avoid buffer-length heuristics for handshake (stateful parsing is required).
