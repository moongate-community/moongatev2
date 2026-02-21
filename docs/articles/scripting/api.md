# Scripting API Reference

Complete reference for the Moongate v2 Lua scripting API.

## Global Modules

### log - Logging Module

```lua
log.debug(message: string)      -- Debug level logging
log.info(message: string)       -- Info level logging
log.warning(message: string)    -- Warning level logging
log.error(message: string)      -- Error level logging
log.critical(message: string)   -- Critical level logging
```

**Example:**
```lua
log.debug("Debug information for developers")
log.info("Server started successfully")
log.warning("Low memory warning")
log.error("Failed to load template")
log.critical("Database connection lost")
```

### server - Server Module

```lua
server.broadcast(message: string)                    -- Broadcast to all players
server.get_player_count(): number                    -- Get active player count
server.get_player(serial: number): Player|nil        -- Get player by serial
server.get_players(): table                          -- Get all players
server.shutdown()                                    -- Graceful shutdown
server.save_world()                                  -- Save world state
server.get_uptime(): number                          -- Get server uptime in seconds
server.get_version(): string                         -- Get server version
```

**Example:**
```lua
-- Broadcast message
server.broadcast("Server will restart in 5 minutes!")

-- Get player count
local count = server.get_player_count()
log.info("Players online: " .. count)

-- Get specific player
local player = server.get_player(0x00000001)
if player then
    log.info("Found player: " .. player.Name)
end
```

### game - Game Module

```lua
game.spawn_mobile(body: number, hue: number, x: number, y: number, z: number, map: number): number
game.spawn_item(itemId: number, amount: number, x: number, y: number, z: number): number
game.spawn_npc(npcId: string, x: number, y: number, z: number): number
game.get_mobile(serial: number): Mobile|nil          -- Get mobile by serial
game.get_item(serial: number): Item|nil              -- Get item by serial
game.move_object(serial: number, x: number, y: number, z: number): boolean
game.delete_object(serial: number): boolean          -- Delete object
game.get_distance(obj1: number, obj2: number): number
game.get_objects_in_range(x: number, y: number, z: number, range: number): table
```

**Example:**
```lua
-- Spawn a mobile
local mobileSerial = game.spawn_mobile(0x0190, 0, 1000, 2000, 0, 0)

-- Spawn an item
local itemSerial = game.spawn_item(0x0E76, 1, 1000, 2000, 0)

-- Move object
local success = game.move_object(mobileSerial, 1001, 2000, 0)

-- Get distance
local distance = game.get_distance(playerSerial, targetSerial)
if distance > 10 then
    log.warning("Target too far away")
end
```

### player - Player Module

```lua
player.send_message(serial: number, text: string)              -- Send message to player
player.send_gump(serial: number, gumpId: number, data: table)  -- Send gump dialog
player.teleport(serial: number, x: number, y: number, z: number, map: number)
player.add_item(serial: number, itemId: number, amount: number): number
player.remove_item(serial: number, itemSerial: number, amount: number): boolean
player.get_skill(serial: number, skillName: string): number
player.set_skill(serial: number, skillName: string, value: number)
player.get_stats(serial: number): table                        -- Get str/dex/int
player.set_stats(serial: number, stats: table)                 -- Set str/dex/int
player.send_sound(serial: number, soundId: number)             -- Play sound
player.send_effect(serial: number, effectId: number, target: number)
```

**Example:**
```lua
-- Send message
player.send_message(playerSerial, "Welcome to the server!")

-- Teleport player
player.teleport(playerSerial, 5000, 1000, 0, 0)

-- Add item to backpack
local backpackItem = player.add_item(playerSerial, 0x0E76, 1)

-- Get/Set skills
local magery = player.get_skill(playerSerial, "magery")
player.set_skill(playerSerial, "magery", 100.0)

-- Get stats
local stats = player.get_stats(playerSerial)
log.info("STR: " .. stats.Strength .. ", DEX: " .. stats.Dexterity .. ", INT: " .. stats.Intelligence)
```

### world - World Module

```lua
world.get_time(): table                         -- Get server time {year, month, day, hour, minute, second}
world.get_tile(x: number, y: number, z: number, map: number): table
world.get_region(x: number, y: number, map: number): string
world.get_weather(): table                      -- Get current weather
world.set_weather(weatherType: number, duration: number)
world.spawn_npc(npcId: string, x: number, y: number, z: number, map: number): number
world.despawn(serial: number): boolean
world.is_day(): boolean                         -- Check if it's daytime
world.get_players_in_region(region: string): table
```

**Example:**
```lua
-- Get time
local time = world.get_time()
log.info(string.format("Time: %02d:%02d:%02d", time.hour, time.minute, time.second))

-- Get tile info
local tile = world.get_tile(1000, 2000, 0, 0)
log.info("Tile ID: " .. tile.Id .. ", Z: " .. tile.Z)

-- Get region
local region = world.get_region(1000, 2000, 0)
log.info("Region: " .. region)

-- Set weather
world.set_weather(2, 300)  -- Rain for 5 minutes
```

### commands - Commands Module

```lua
commands.register(name: string, handler: function)           -- Register chat command
commands.unregister(name: string)                            -- Unregister command
commands.process(playerSerial: number, text: string): boolean, any
commands.list(): table                                       -- List all commands
```

**Example:**
```lua
-- Register command
commands.register("teleport", function(playerSerial, args)
    local player = server.get_player(playerSerial)
    if not player.IsAdmin then
        return false, "Access denied"
    end
    
    local x, y, z = args:match("(%d+) (%d+) (%d+)")
    player.teleport(playerSerial, tonumber(x), tonumber(y), tonumber(z))
    return true
end)

-- Unregister command
commands.unregister("teleport")

-- List commands
local cmds = commands.list()
for _, cmd in ipairs(cmds) do
    log.info("Command: " .. cmd)
end
```

## Data Types

### Player

```lua
Player = {
    Serial: number,           -- Unique identifier
    Name: string,             -- Character name
    Account: string,          -- Account username
    Position: Point3D,        -- Current position
    IsAdmin: boolean,         -- Admin flag
    IsModerator: boolean,     -- Moderator flag
    IsInWorld: boolean,       -- In world flag
    LastActivity: number      -- Last activity timestamp
}
```

### Mobile

```lua
Mobile = {
    Serial: number,           -- Unique identifier
    Name: string,             -- Mobile name
    Body: number,             -- Body ID
    Hue: number,              -- Hue color
    Position: Point3D,        -- Current position
    Map: number,              -- Map facet
    Hits: number,             -- Current hits
    HitsMax: number,          -- Maximum hits
    Stamina: number,          -- Current stamina
    StaminaMax: number,       -- Maximum stamina
    Mana: number,             -- Current mana
    ManaMax: number,          -- Maximum mana
    Direction: number,        -- Facing direction
    WarMode: boolean,         -- War mode flag
    Paralyzed: boolean,       -- Paralyzed flag
    Poisoned: boolean         -- Poisoned flag
}
```

### Item

```lua
Item = {
    Serial: number,           -- Unique identifier
    ItemId: number,           -- Item graphic ID
    Amount: number,           -- Stack amount
    Hue: number,              -- Hue color
    Position: Point3D,        -- Position (if world item)
    ParentSerial: number|nil, -- Parent container serial
    Layer: number|nil,        -- Equip layer (if equipped)
    IsMovable: boolean,       -- Can be picked up
    IsContainer: boolean      -- Is a container
}
```

### Point3D

```lua
Point3D = {
    X: number,                -- X coordinate
    Y: number,                -- Y coordinate
    Z: number                 -- Z coordinate
}
```

### Map

```lua
Map = {
    Felucca = 0,              -- Felucca facet
    Trammel = 1,              -- Trammel facet
    Ilshenar = 2,             -- Ilshenar facet
    Malas = 3,                -- Malas facet
    Tokuno = 4,               -- Tokuno facet
    TerMur = 5                -- TerMur facet
}
```

## Callbacks

### Server Callbacks

```lua
function on_server_start()              -- Called when server starts
function on_server_stop()               -- Called when server stops
function on_tick()                      -- Called every game tick
function on_save_world()                -- Called before world save
```

### Player Callbacks

```lua
function on_player_connected(player)    -- Player connected
function on_player_disconnected(player) -- Player disconnected
function on_player_speech(player, text) -- Player spoke
function on_player_login(player)        -- Player logged in
function on_player_logout(player)       -- Player logged out
function on_player_use_item(player, item) -- Player used item
function on_player_equip_item(player, item) -- Player equipped item
function on_player_combat_hit(attacker, defender, damage) -- Combat hit
```

### World Callbacks

```lua
function on_mobile_created(mobile)      -- Mobile created
function on_mobile_deleted(mobile)      -- Mobile deleted
function on_item_created(item)          -- Item created
function on_item_deleted(item)          -- Item deleted
function on_weather_changed(weather)    -- Weather changed
```

## Utility Functions

### String Utilities

```lua
string.split(str: string, delimiter: string): table
string.trim(str: string): string
string.starts_with(str: string, prefix: string): boolean
string.ends_with(str: string, suffix: string): boolean
string.contains(str: string, substr: string): boolean
```

### Table Utilities

```lua
table.contains(tbl: table, value: any): boolean
table.keys(tbl: table): table
table.values(tbl: table): table
table.length(tbl: table): number
table.merge(tbl1: table, tbl2: table): table
```

### Math Utilities

```lua
math.distance(x1: number, y1: number, x2: number, y2: number): number
math.clamp(value: number, min: number, max: number): number
math.lerp(a: number, b: number, t: number): number
math.random_range(min: number, max: number): number
```

## Error Handling

### pcall for Safe Calls

```lua
local success, result = pcall(function()
    return game.spawn_mobile(0x0190, 0, 1000, 2000, 0)
end)

if not success then
    log.error("Failed to spawn mobile: " .. tostring(result))
else
    log.info("Spawned mobile with serial: " .. result)
end
```

### xpcall with Error Handler

```lua
local function error_handler(err)
    log.error("Script error: " .. tostring(err))
    return err
end

local success, result = xpcall(function()
    risky_operation()
end, error_handler)
```

## Best Practices

### Performance

```lua
-- GOOD: Cache function references
local log_info = log.info
local game_spawn = game.spawn_mobile

for i = 1, 10 do
    log_info("Spawning mobile " .. i)
    game_spawn(0x0190, 0, 1000 + i, 2000, 0)
end

-- BAD: Repeated lookups
for i = 1, 10 do
    log.info("Spawning mobile " .. i)
    game.spawn_mobile(0x0190, 0, 1000 + i, 2000, 0)
end
```

### Memory Management

```lua
-- GOOD: Clear tables when done
local large_table = {}
for i = 1, 10000 do
    large_table[i] = {data = i}
end

-- Process table
process_data(large_table)

-- Clear for GC
large_table = nil

-- BAD: Memory leak
local persistent_table = {}
function on_tick()
    persistent_table[#persistent_table + 1] = {tick = world.get_time()}
end
```

## Next Steps

- **[Modules](modules.md)** - Create custom modules
- **[Overview](overview.md)** - Scripting introduction
- **[Persistence](../persistence/overview.md)** - Data storage

---

**Previous**: [Modules](modules.md) | **Next**: [Persistence Overview](../persistence/overview.md)
