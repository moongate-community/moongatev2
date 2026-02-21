<p align="center">
  <img src="images/moongate_logo.png" alt="Moongate logo" width="240" />
</p>

All notable changes to this project will be documented in this file. See [versionize](https://github.com/versionize/versionize) for commit guidelines.

<a name="0.7.1"></a>
## [0.7.1](https://www.github.com/moongate-community/moongatev2/releases/tag/v0.7.1) (2026-02-21)

### Features

* add PacketSenderService with dedicated sender thread ([24429c6](https://www.github.com/moongate-community/moongatev2/commit/24429c6dd143e9f2404094edaf4ee4d94b09ed02))
* add synchronous Send method to IOutboundPacketSender ([f943324](https://www.github.com/moongate-community/moongatev2/commit/f9433240bcaae74692a3437f47aaee53f167710c))
* change default tick duration from 250ms to 8ms ([1030f8f](https://www.github.com/moongate-community/moongatev2/commit/1030f8f434abb6a9f8e86b30ef0352d8c4d3b091))
* switch OutgoingPacketQueue to Channel<T> with WaitToReadAsync ([f69a93b](https://www.github.com/moongate-community/moongatev2/commit/f69a93bcedeb172b2f42a85e5a8a5f4690e3f015))
* **loop:** switch to timestamp-driven timer updates with idle cpu throttle ([6f24593](https://www.github.com/moongate-community/moongatev2/commit/6f24593cdbccfaa209f286814edc1a69bec25d3d))
* **metrics:** add mvp loop/network/timer/persistence runtime metrics ([c8b1a08](https://www.github.com/moongate-community/moongatev2/commit/c8b1a086617da1b8e1346b142db3e34b671b472e))
* **metrics:** generate metric samples from annotated snapshots ([d193295](https://www.github.com/moongate-community/moongatev2/commit/d1932951e45a0c259834d01abbfd9b906ad4a46d))
* **network:** parse get player status packet and add tests ([bc80cf2](https://www.github.com/moongate-community/moongatev2/commit/bc80cf2ef4a69eb0fcc552914db5e4db94fa8c00))
* **server:** add movement flow, paperdoll updates, and timer tick alignment ([e57e1ec](https://www.github.com/moongate-community/moongatev2/commit/e57e1ec08d1a1ba3ad83bbcf502dcc358e8f1729))
* **server:** add movement throttling and run/walk speed handling ([a0a1303](https://www.github.com/moongate-community/moongatev2/commit/a0a130381a718e79aa068cf46fa6722b3154f327))
* **status:** implement 0x11 basic player status response ([45abfbd](https://www.github.com/moongate-community/moongatev2/commit/45abfbd45b687fdae8bae4df0296ceb651633981))

### Bug Fixes

* count all executed callbacks in metrics including failures ([63ce044](https://www.github.com/moongate-community/moongatev2/commit/63ce044a692b65b0882c9f92e5920b7ae224966f))
* make BaseMoongateService StartAsync/StopAsync virtual and use override in services ([7ed4293](https://www.github.com/moongate-community/moongatev2/commit/7ed429306699fd120d12c25a033391464c56ab09))
* use Task.WhenAll in packet dispatch instead of fire-and-forget ([b3cba93](https://www.github.com/moongate-community/moongatev2/commit/b3cba93d2b6cb20b3c51e9bc5d49716b156023d4))
* **network:** align movement and mobile incoming hair serialization ([9fb5905](https://www.github.com/moongate-community/moongatev2/commit/9fb5905f9b511f90f8a35bfb2172e5f862775dab))

<a name="0.7.0"></a>
## [0.7.0](https://www.github.com/moongate-community/moongatev2/releases/tag/v0.7.0) (2026-02-20)

### Features

* expand server/persistence templates, entities and tests ([14214fb](https://www.github.com/moongate-community/moongatev2/commit/14214fbf5c53ed564a98035ea4e1c7616c2feba8))

<a name="0.6.0"></a>

## [0.6.0](https://www.github.com/moongate-community/moongatev2/releases/tag/v0.6.0) (2026-02-20)

### Features

- **events,uo:** add outbound listener base and runtime equipment references ([2f4c147](https://www.github.com/moongate-community/moongatev2/commit/2f4c14740c3c433f3e2989cf5791ae3a08733875))
- **network:** add ping packet handling and movement/general info packets ([ceb67e8](https://www.github.com/moongate-community/moongatev2/commit/ceb67e83846940c1fd730fc726c8da16c7258bfd))
- **network-packets:** add after-login outgoing packet serialization ([63f75f4](https://www.github.com/moongate-community/moongatev2/commit/63f75f4cabac81067be1612e98bbf9f5320cefc6))
- **server:** implement login character packet flow ([0407571](https://www.github.com/moongate-community/moongatev2/commit/04075715e92256f386c01c7cdeecd2c9a047cee6))

### Bug Fixes

- **logging:** avoid timestamp highlighting collisions in console sink ([c261c89](https://www.github.com/moongate-community/moongatev2/commit/c261c89a330928677b09739159d17d33cce50cb4))

<a name="0.5.0"></a>

## [0.5.0](https://www.github.com/moongate-community/moongatev2/releases/tag/v0.5.0) (2026-02-19)

### Features

- add console ui logger with prompt and spectre colors ([8e7a9e6](https://www.github.com/moongate-community/moongatev2/commit/8e7a9e6d75898f6a8c4783c58ddb6bc9ee093ed9))
- **console:** add scrollable log history in interactive UI ([45f61f7](https://www.github.com/moongate-community/moongatev2/commit/45f61f728701fd8c295505efd1fb4cba8d1b2327))
- **console:** add startup input lock with unlock flow ([45e7e67](https://www.github.com/moongate-community/moongatev2/commit/45e7e6752e3ddd9ae5a6b835b6d1de9b0d70ced5))
- **network:** add CharactersStartingLocationsPacket (0xA9) ([c3cf68d](https://www.github.com/moongate-community/moongatev2/commit/c3cf68df681ba2d156aa77fbce0da60510414b90))
- **persistence:** add repository count APIs and bootstrap account check ([d46676e](https://www.github.com/moongate-community/moongatev2/commit/d46676ef54363acc85b09d3757caa7fb0cf50e65))
- **scripting:** cache compiled lua chunks ([67af7cb](https://www.github.com/moongate-community/moongatev2/commit/67af7cb18cec0a9fe6a665d705f4c6c8c5a47d35))
- **server:** add command system service and lifecycle shutdown flow ([8651e1a](https://www.github.com/moongate-community/moongatev2/commit/8651e1a1dc9fab3d03a87c6e8a8226884479b6d3))
- **server:** add timer metrics, item/mobile link refs, and docs status refresh ([3378403](https://www.github.com/moongate-community/moongatev2/commit/33784038b91f186d958dacdbbeb15d6903690e51))
- **server:** refactor metrics into snapshot sources ([bf39de0](https://www.github.com/moongate-community/moongatev2/commit/bf39de08782bd9eab2a8d7ad75906d5d74d16788))
- **server:** update persistence/timers, packet mapping and serial parsing fixes ([0974549](https://www.github.com/moongate-community/moongatev2/commit/0974549e25ec3e79921ebb22e23ed4720fd79dae))
- **uo:** add mobile stat recalculation and apply on character mapping ([3d93590](https://www.github.com/moongate-community/moongatev2/commit/3d935908fab8d169afbe2826b49f851c3fb62543))

### Bug Fixes

- align docker publish inputs and slim http builder for aot ([69a68f1](https://www.github.com/moongate-community/moongatev2/commit/69a68f18fe9881689e2874e3bb200803a4718e57))
- **network:** enforce ordered outbound send and always compress post-login ([c6b263b](https://www.github.com/moongate-community/moongatev2/commit/c6b263b72129c6e858b6f3fea30619648e4068f1))

<a name="0.4.0"></a>

## [0.4.0](https://www.github.com/moongate-community/moongatev2/releases/tag/v0.4.0) (2026-02-19)

### Features

- **network:** add client middleware management and support features packet ([6c8f0fa](https://www.github.com/moongate-community/moongatev2/commit/6c8f0fa74a2545f09d4a62d9d4d3e735c6424d66))

<a name="0.3.0"></a>

## [0.3.0](https://www.github.com/moongate-community/moongatev2/releases/tag/v0.3.0) (2026-02-19)

### Features

- **http:** add embedded HTTP host service with options and dedicated logging ([033cd6d](https://www.github.com/moongate-community/moongatev2/commit/033cd6df52360aec8d72ae08192baccc942fe507))
- **network:** add client middleware management and support features packet ([6c8f0fa](https://www.github.com/moongate-community/moongatev2/commit/6c8f0fa74a2545f09d4a62d9d4d3e735c6424d66))
- **network:** handle reconnect seed handshake and refine login packet flow ([529dc79](https://www.github.com/moongate-community/moongatev2/commit/529dc79a70d384f2d5bac02e8db82f994d6f0747))
- **packets,scripting:** generate packet definitions and fix Lua log module interop ([71f3658](https://www.github.com/moongate-community/moongatev2/commit/71f3658df446e641ca5139d2c66b5373a35503dc))
- **server:** add http config and json context plumbing ([a94ea67](https://www.github.com/moongate-community/moongatev2/commit/a94ea674dacdeb0398e6e4a16c151ce8bd5c946a))
- **server:** add timer wheel service and game-event script bridge with tests ([de14394](https://www.github.com/moongate-community/moongatev2/commit/de143947a0f5518b965215fe90b1bcf6c34de466))

<a name="0.2.0"></a>

## [0.2.0](https://www.github.com/moongate-community/moongatev2/releases/tag/v0.2.0) (2026-02-17)

### Features

- **server:** bootstrap bundled data assets into data directory ([2929ce1](https://www.github.com/moongate-community/moongatev2/commit/2929ce1f1a62a64947733a8312f04b150d2a6efc))
- **server:** wire UO file loaders and typed json context ([07551a2](https://www.github.com/moongate-community/moongatev2/commit/07551a2f200a882169af7e588d558a34358da63b))

### Bug Fixes

- **server:** resolve default root directory from app base path ([7aaa217](https://www.github.com/moongate-community/moongatev2/commit/7aaa21719dc5b834476369d2ef827de71b9a1487))

<a name="0.1.0"></a>

## [0.1.0](https://www.github.com/moongate-community/moongatev2/releases/tag/v0.1.0) (2026-02-17)

### Features

- add abstractions, network project, and Obsidian tooling setup ([300d338](https://www.github.com/moongate-community/moongatev2/commit/300d33837531512a083888dc7d03636112b31acd))
- add core and server projects to solution ([d1e2f47](https://www.github.com/moongate-community/moongatev2/commit/d1e2f47efb795c6c1b6b2a19df58578065e02002))
- **abstractions:** add service project wiring and sprint kanban ([d428991](https://www.github.com/moongate-community/moongatev2/commit/d428991c4581cfbd450be2036d574570821b3264))
- **core:** add utility, json, and configuration foundations ([2868a72](https://www.github.com/moongate-community/moongatev2/commit/2868a72d5fa52cc071563383e42177cd775a6a53))
- **network:** add packet registry with generated packet table ([9ba6051](https://www.github.com/moongate-community/moongatev2/commit/9ba60510191e7df730d8758e39a0d5c82481f0b9))
- **network:** add span io and base packet parsing infrastructure ([89be21b](https://www.github.com/moongate-community/moongatev2/commit/89be21bea67b65b57b9649a1aa00ad3803cc185c))
- **network:** add tcp client pipeline, events, and buffer exceptions ([97ff80f](https://www.github.com/moongate-community/moongatev2/commit/97ff80f89d7991828dee4573a558889fec365018))
- **network:** add tcp server foundation with buffers and compression ([d3bed53](https://www.github.com/moongate-community/moongatev2/commit/d3bed53c02d88b83bc1e321a7b99fefbc2a7b91b))
- **packets:** add packet descriptions from attributes and fix aot publish script ([03b6b52](https://www.github.com/moongate-community/moongatev2/commit/03b6b528eb4153b4f1285e94e9c846ed9def5b3a))
- **packets:** organize incoming packets by domain ([aaad0bc](https://www.github.com/moongate-community/moongatev2/commit/aaad0bcc68503b217f78b71a812706430095e582))
- **server:** add lifecycle run loop and align packet metadata ([c24abca](https://www.github.com/moongate-community/moongatev2/commit/c24abca925384b360e4e455f2d9025dae344fdea))
- **server:** add message bus and domain event bus infrastructure ([03f605c](https://www.github.com/moongate-community/moongatev2/commit/03f605cbd1bfd4c2008d601ca24b3d0f9ec870d9))
- **server:** add moongate bootstrap registration ([39ec37e](https://www.github.com/moongate-community/moongatev2/commit/39ec37e718045a4955e8f9177d112a5bf0c72989))
- **server:** add packet data dump logging with dedicated sink ([12c9c81](https://www.github.com/moongate-community/moongatev2/commit/12c9c81b1e0115590bb96bd38f2d1f909cdc259b))
- **server:** add startup header resource ([8f68586](https://www.github.com/moongate-community/moongatev2/commit/8f685865c37d09972dfd399f0e1decee030887fc))
- **server:** implement game loop lifecycle and tests ([853c4fd](https://www.github.com/moongate-community/moongatev2/commit/853c4fd86a3bd0b0a52daa00ff6f8e3c7c3c359c))
- **server:** scaffold game loop service contracts and models ([33f1cbe](https://www.github.com/moongate-community/moongatev2/commit/33f1cbe849a3eee8ad945153377d7defc01bf649))
- **server:** scaffold network packet listener and service contracts ([f1cea7b](https://www.github.com/moongate-community/moongatev2/commit/f1cea7b69e8ab473815438c9ad08fbcd7fb6d9af))
- **server:** wire startup banner and platform handling updates ([6e9af34](https://www.github.com/moongate-community/moongatev2/commit/6e9af3447d8878ac1239ef6277e580f4a4ea77ec))
- **uo-data:** add Serial type and coverage tests ([69f5836](https://www.github.com/moongate-community/moongatev2/commit/69f5836095b265d0d843fbe4d7ba723b98cc8731))
- **uo-data:** import core legacy UO data and add minimal entity model ([4359964](https://www.github.com/moongate-community/moongatev2/commit/43599640fb4b9a84fd0185c78580f876d10d003f))
