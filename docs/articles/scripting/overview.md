# Lua Scripting

Moongate v2 includes a powerful Lua scripting subsystem for gameplay customization.

## Overview

The scripting system is built on **MoonSharp**, a lightweight Lua interpreter for .NET. It provides:

- Full Lua 5.2 compatibility
- .NET interop via attributes
- Automatic `.luarc` generation for editor tooling
- Callback system for game events

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Scripting System                          │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐   │
│  │ Lua Scripts  │───▶│  Script      │───▶│  .NET        │   │
│  │ (.lua files) │    │  Engine      │    │  Modules     │   │
│  └──────────────┘    └──────────────┘    └──────────────┘   │
│                             │                    │           │
│                             │                    ▼           │
│                        ┌────┴────┐    ┌──────────────┐      │
│                        │ .luarc  │    │  Game        │      │
│                        │ Generator│   │  Events      │      │
│                        └─────────┘    └──────────────┘      │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## Quick Start

### Create Your First Script

Create `scripts/init.lua`:

```lua
-- Called when a player connects
function on_player_connected(player)
    log.info("Player connected: " .. player.Name)
end

-- Called when a player disconnects
function on_player_disconnected(player)
    log.info("Player disconnected: " .. player.Name)
end
```

### Create a Script Module

Create a .NET module to expose to Lua:

```csharp
using Moongate.Scripting.Attributes;

[ScriptModule("server")]
public sealed class ServerModule
{
    private readonly ILogger _logger;
    
    public ServerModule(ILogger logger)
    {
        _logger = logger;
    }
    
    [ScriptFunction("broadcast")]
    public void Broadcast(string message)
    {
        _logger.LogInformation("Broadcast: {Message}", message);
        // Send to all players...
    }
    
    [ScriptFunction("get_player_count")]
    public int GetPlayerCount()
    {
        return _sessionManager.ActiveCount;
    }
}
```

### Use Module in Lua

```lua
-- Broadcast to all players
server.broadcast("Welcome to Moongate v2!")

-- Get player count
local count = server.get_player_count()
log.info("Active players: " .. count)
```

## Script Modules

### Defining Modules

Modules are .NET classes exposed to Lua:

```csharp
using Moongate.Scripting.Attributes;

[ScriptModule("game")]
public sealed class GameModule
{
    [ScriptFunction("spawn_mobile")]
    public Serial SpawnMobile(int bodyId, int hue, Point3D location)
    {
        // Spawn mobile logic
        return mobile.Serial;
    }
    
    [ScriptFunction("spawn_item")]
    public Serial SpawnItem(int itemId, int amount, Point3D location)
    {
        // Spawn item logic
        return item.Serial;
    }
    
    [ScriptFunction("get_time")]
    public DateTime GetTime()
    {
        return DateTime.UtcNow;
    }
}
```

### ScriptFunction Attributes

```csharp
[ScriptFunction("name")]  // Expose function with custom name
[ScriptFunction("name", IsAsync = true)]  // Async function
[ScriptFunction("name", RequirePlayer = true)]  // Auto-inject player
```

### ScriptConstant Attributes

```csharp
[ScriptConstant("VERSION")]
public string Version => "0.7.0";

[ScriptConstant("MAX_PLAYERS")]
public int MaxPlayers => 1000;
```

## Callbacks

### Available Callbacks

```lua
-- Player events
function on_player_connected(player) end
function on_player_disconnected(player) end
function on_player_speech(player, text) end
function on_player_use_item(player, item) end

-- World events
function on_server_start() end
function on_server_stop() end
function on_tick() end  -- Called every game tick
```

### Callback Parameters

```lua
function on_player_speech(player, text)
    -- player: { Serial, Name, Position, Account }
    -- text: string
    
    -- Log speech
    log.info(player.Name .. " says: " .. text)
    
    -- Process commands
    if text:starts_with("/") then
        process_command(player, text)
    end
end
```

## API Reference

### Log Module

```lua
log.debug(message)      -- Debug level
log.info(message)       -- Info level
log.warning(message)    -- Warning level
log.error(message)      -- Error level
log.critical(message)   -- Critical level
```

### Server Module

```lua
server.broadcast(message)           -- Broadcast to all players
server.get_player_count()           -- Get active player count
server.get_player(serial)           -- Get player by serial
server.shutdown()                   -- Graceful shutdown
server.save_world()                 -- Save world state
```

### Game Module

```lua
game.spawn_mobile(body, hue, x, y, z, map)  -- Spawn mobile
game.spawn_item(itemId, amount, x, y, z)    -- Spawn item
game.get_mobile(serial)                     -- Get mobile data
game.get_item(serial)                       -- Get item data
game.move_object(serial, x, y, z)           -- Move object
game.delete_object(serial)                  -- Delete object
```

### Player Module

```lua
player.send_message(text)           -- Send message to player
player.send_gump(gumpId, data)      -- Send gump dialog
player.teleport(x, y, z, map)       -- Teleport player
player.add_item(itemId, amount)     -- Add item to backpack
player.remove_item(serial, amount)  -- Remove item
player.get_skill(skillName)         -- Get skill value
player.set_skill(skillName, value)  -- Set skill value
```

### World Module

```lua
world.get_time()                    -- Get server time
world.get_tile(x, y, z, map)        -- Get tile info
world.get_region(x, y, map)         -- Get region name
world.spawn_npc(mobileId, x, y, z)  -- Spawn NPC
world.despawn(serial)               -- Despawn object
```

## Configuration

### Script Settings

```json
{
  "scripting": {
    "enabled": true,
    "scriptsDirectory": "scripts",
    "autoReload": false,
    "debugMode": false,
    "timeoutMilliseconds": 5000
  }
}
```

### Script Directories

Scripts are loaded from:

```
scripts/
├── init.lua              # Main entry point
├── commands/             # Command handlers
│   ├── admin.lua
│   └── player.lua
├── events/               # Event handlers
│   ├── combat.lua
│   └── trade.lua
└── modules/              # Custom Lua modules
    └── utils.lua
```

## Editor Tooling

### .luarc.json Generation

Moongate v2 automatically generates `.luarc.json` for editor support:

```json
{
  "workspace.library": [
    "/path/to/moongatev2/scripts/definitions"
  ],
  "diagnostics.disable": [],
  "runtime.version": "Lua 5.2"
}
```

### TypeScript-like Definitions

Auto-generated `definitions.lua`:

```lua
---@class Player
---@field Serial number
---@field Name string
---@field Position Position

---@class LogModule
log = {}

---@param message string
function log.debug(message) end

---@param message string
function log.info(message) end

---@class ServerModule
server = {}

---@param message string
function server.broadcast(message) end

---@return number
function server.get_player_count() end
```

### VS Code Setup

1. Install **Lua Language Server** extension
2. Open scripts folder in VS Code
3. Definitions are auto-generated on server start
4. Enjoy IntelliSense and type checking!

## Error Handling

### Script Errors

```csharp
try
{
    _luaEngine.CallFunction("on_player_connected", player);
}
catch (ScriptRuntimeException ex)
{
    logger.LogError(ex, "Script error in on_player_connected");
}
```

### Timeout Protection

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
try
{
    await _luaEngine.ExecuteAsync(script, cts.Token);
}
catch (OperationCanceledException)
{
    logger.LogWarning("Script execution timed out");
}
```

## Performance

### Best Practices

**DO:**
- Cache function references
- Use local variables
- Minimize .NET interop calls
- Batch operations

**DON'T:**
- Create tables in loops
- Use global variables excessively
- Call .NET functions in tight loops
- Block in callbacks

### Example: Efficient Script

```lua
-- GOOD: Cached references
local log_info = log.info
local server_broadcast = server.broadcast

local function process_player(player)
    local name = player.Name  -- Cache property
    log_info("Processing: " .. name)
end

-- BAD: Repeated lookups
function process_player(player)
    log.info("Processing: " .. player.Name)
    server.broadcast("Processing: " .. player.Name)
end
```

## Testing

### Unit Testing Scripts

```csharp
[Fact]
public void Script_OnPlayerConnected_CallsLogInfo()
{
    var engine = CreateScriptEngine();
    var mockLogger = new Mock<ILogger>();
    
    engine.RegisterModule("log", mockLogger.Object);
    engine.LoadScript("init.lua");
    
    engine.CallFunction("on_player_connected", testPlayer);
    
    mockLogger.Verify(l => l.Info(It.IsAny<string>()), Times.Once);
}
```

## Examples

### Custom Command

```lua
-- scripts/commands/admin.lua

function cmd_teleport(player, targetSerial)
    if not player.IsAdmin then
        player.send_message("You must be an admin!")
        return
    end
    
    local target = game.get_mobile(targetSerial)
    if not target then
        player.send_message("Target not found!")
        return
    end
    
    player.teleport(target.Position.X, target.Position.Y, target.Position.Z)
    log.info("Admin " .. player.Name .. " teleported to " .. target.Name)
end
```

### Custom NPC

```lua
-- scripts/npcs/merchant.lua

local merchant = {
    Name = "Bob the Merchant",
    Body = 0x0190,
    Hue = 0,
    Speech = {
        "Welcome to my shop!",
        "Looking for good deals?",
        "I have the best prices in Britannia!"
    }
}

function on_speech(player, text)
    for _, phrase in ipairs(merchant.Speech) do
        if text:find(phrase, 1, true) then
            player.send_message(merchant.Name .. ": " .. phrase)
            return
        end
    end
end
```

## Next Steps

- **[Script Modules](modules.md)** - Create custom modules
- **[API Reference](api.md)** - Full API documentation
- **[Persistence](../persistence/overview.md)** - Data storage

---

**Previous**: [Solution Structure](../architecture/solution.md) | **Next**: [Script Modules](modules.md)
